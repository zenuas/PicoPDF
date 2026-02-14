using System;
using System.Diagnostics;

namespace PicoPDF.OpenType.Tables.PostScript;

public static class Subroutine
{
    public static void EnumSubroutines(Span<byte> charstring, byte[][]? local_subr, byte[][] global_subr, Func<bool, int, bool> f)
    {
        var local_bias = GetSubroutineBias(local_subr?.Length ?? 0);
        var global_bias = GetSubroutineBias(global_subr.Length);
        var operand = 0;
        for (var i = 0; i < charstring.Length; i++)
        {
            var c = charstring[i];
            if (c is (byte)CharstringCommandCodes.Callsubr or (byte)CharstringCommandCodes.Callgsubr)
            {
                if (c == (byte)CharstringCommandCodes.Callgsubr)
                {
                    var index = operand + global_bias;
                    Debug.Assert(index >= 0 && index < global_subr.Length);
                    if (index >= 0 && index < global_subr.Length && f(true, index)) EnumSubroutines(global_subr[index], null, global_subr, f);
                }
                else if (local_subr is { })
                {
                    var index = operand + local_bias;
                    Debug.Assert(index >= 0 && index < local_subr.Length);
                    if (index >= 0 && index < local_subr.Length && f(false, index)) EnumSubroutines(local_subr[index], local_subr, global_subr, f);
                }
                operand = 0;
            }
            else if (c != 28 && c < 32)
            {
                operand = 0; // any operator
                if (c == (byte)CharstringCommandCodes.Escape) i++;
            }
            else
            {
                operand = CharstringNumber(charstring[i..Math.Min(charstring.Length, 1 + (i += NextNumberBytes(c)))]); // any number
            }
        }
    }

    public static int GetSubroutineBias(int subr_count) =>
        subr_count < 1240 ? 107 :
        subr_count < 33900 ? 1131 :
        32768;

    public static int CharstringNumber(Span<byte> charstring) =>
        charstring.Length == 1 && charstring[0] is >= 32 and <= 246 ? charstring[0] - 139 :
        charstring.Length == 2 && charstring[0] is >= 247 and <= 250 ? (charstring[0] - 247) * 256 + charstring[1] + 108 :
        charstring.Length == 2 && charstring[0] is >= 251 and <= 254 ? -((charstring[0] - 251) * 256) - charstring[1] - 108 :
        charstring.Length == 3 && charstring[0] == 28 ? (short)(charstring[1] << 8 | charstring[2]) :
        charstring.Length == 5 && charstring[0] == 255 ? charstring[1] << 24 | charstring[2] << 16 | charstring[3] << 8 | charstring[4] :
        0;

    public static int NextNumberBytes(byte c) =>
        c == 255 ? 4 :
        c == 28 ? 2 :
        c <= 246 ? 0 :
        1;
}
