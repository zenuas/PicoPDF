using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PicoPDF.OpenType.Tables.PostScript;

public static class Subroutine
{
    public static void EnumSubroutines(byte[] charstring, byte[][] local_subr, byte[][] global_subr, Func<bool, int, bool> f)
    {
        var local_bias = GetSubroutineBias(local_subr.Length);
        var global_bias = GetSubroutineBias(global_subr.Length);
        var bytes = new List<byte>();
        var num_escape = 0;
        for (var i = 0; i < charstring.Length; i++)
        {
            var c = charstring[i];
            if (num_escape > 0)
            {
                bytes.Add(c);
                num_escape--;
            }
            else if (c == (byte)CharstringCommandCodes.Escape)
            {
                i++;
                bytes.Clear();
            }
            else if (c is (byte)CharstringCommandCodes.Callsubr or (byte)CharstringCommandCodes.Callgsubr)
            {
                var num = CharstringNumber(bytes);
                if (c == (byte)CharstringCommandCodes.Callsubr)
                {
                    var index = num + local_bias;
                    Debug.Assert(index >= 0 && index < local_subr.Length);
                    if (index >= 0 && index < local_subr.Length && f(false, index)) EnumSubroutines(local_subr[index], local_subr, global_subr, f);
                }
                else
                {
                    var index = num + global_bias;
                    Debug.Assert(index >= 0 && index < global_subr.Length);
                    if (index >= 0 && index < global_subr.Length && f(true, index)) EnumGlobalSubroutines(global_subr[index], global_subr, f);
                }
                bytes.Clear();
            }
            else if (c != 28 && c < 32)
            {
                bytes.Clear(); // any operator
            }
            else
            {
                bytes.Clear(); // any number
                bytes.Add(c);
                num_escape = NextNumberBytes(c);
            }
        }
    }

    public static void EnumGlobalSubroutines(byte[] charstring, byte[][] global_subr, Func<bool, int, bool> f)
    {
        var global_bias = GetSubroutineBias(global_subr.Length);
        var bytes = new List<byte>();
        var num_escape = 0;
        for (var i = 0; i < charstring.Length; i++)
        {
            var c = charstring[i];
            if (num_escape > 0)
            {
                bytes.Add(c);
                num_escape--;
            }
            else if (c == (byte)CharstringCommandCodes.Escape)
            {
                i++;
                bytes.Clear();
            }
            else if (c == (byte)CharstringCommandCodes.Callsubr)
            {
                Debug.Fail("call LocalSubr from GlobalSubr");
            }
            else if (c == (byte)CharstringCommandCodes.Callgsubr)
            {
                var index = CharstringNumber(bytes) + global_bias;
                Debug.Assert(index >= 0 && index < global_subr.Length);
                if (index >= 0 && index < global_subr.Length && f(true, index)) EnumGlobalSubroutines(global_subr[index], global_subr, f);
                bytes.Clear();
            }
            else if (c != 28 && c < 32)
            {
                bytes.Clear(); // any operator
            }
            else
            {
                bytes.Clear(); // any number
                bytes.Add(c);
                num_escape = NextNumberBytes(c);
            }
        }
    }

    public static int GetSubroutineBias(int subr_count) =>
        subr_count < 1240 ? 107 :
        subr_count < 33900 ? 1131 :
        32768;

    public static int CharstringNumber(List<byte> charstring) =>
        charstring.Count == 1 && charstring[0] is >= 32 and <= 246 ? charstring[0] - 139 :
        charstring.Count == 2 && charstring[0] is >= 247 and <= 250 ? (charstring[0] - 247) * 256 + charstring[1] + 108 :
        charstring.Count == 2 && charstring[0] is >= 251 and <= 254 ? -((charstring[0] - 251) * 256) - charstring[1] - 108 :
        charstring.Count == 3 && charstring[0] == 28 ? (short)(charstring[1] << 8 | charstring[2]) :
        charstring.Count == 5 && charstring[0] == 255 ? charstring[1] << 24 | charstring[2] << 16 | charstring[3] << 8 | charstring[4] :
        0;

    public static int NextNumberBytes(byte c) =>
        c == 255 ? 4 :
        c == 28 ? 2 :
        c <= 246 ? 0 :
        1;
}
