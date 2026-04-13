using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenType.Tables.PostScript;

public static class Subroutine
{
    public static void EnumOperands(Span<byte> charstring, Action<CharstringCommandCodes, Stack<float>> f)
    {
        var stack = new Stack<float>();
        for (var i = 0; i < charstring.Length; i++)
        {
            var c = charstring[i];
            if (c < 32)
            {
                var ope = (CharstringCommandCodes)(c == (int)CharstringCommandCodes.Escape ? ((int)CharstringCommandCodes.Escape * 100) + charstring[++i] : c);
                switch (ope)
                {
                    case CharstringCommandCodes.Hstem:
                    case CharstringCommandCodes.Vstem:
                    case CharstringCommandCodes.Vmoveto:
                    case CharstringCommandCodes.Rlineto:
                    case CharstringCommandCodes.Hlineto:
                    case CharstringCommandCodes.Vlineto:
                    case CharstringCommandCodes.Rrcurveto:
                    case CharstringCommandCodes.Hstemhm:
                    case CharstringCommandCodes.Hintmask:
                    case CharstringCommandCodes.Cntrmask:
                    case CharstringCommandCodes.Rmoveto:
                    case CharstringCommandCodes.Hmoveto:
                    case CharstringCommandCodes.Vstemhm:
                    case CharstringCommandCodes.Rcurveline:
                    case CharstringCommandCodes.Rlinecurve:
                    case CharstringCommandCodes.Vvcurveto:
                    case CharstringCommandCodes.Hhcurveto:
                    case CharstringCommandCodes.Shortint:
                    case CharstringCommandCodes.Vhcurveto:
                    case CharstringCommandCodes.Hvcurveto:
                        f(ope, stack);
                        stack.Clear();
                        break;

                    case CharstringCommandCodes.And:
                    case CharstringCommandCodes.Or:
                    case CharstringCommandCodes.Not:
                    case CharstringCommandCodes.Abs:
                    case CharstringCommandCodes.Add:
                    case CharstringCommandCodes.Sub:
                    case CharstringCommandCodes.Div:
                    case CharstringCommandCodes.Neg:
                    case CharstringCommandCodes.Eq:
                    case CharstringCommandCodes.Drop:
                    case CharstringCommandCodes.Put:
                    case CharstringCommandCodes.Get:
                    case CharstringCommandCodes.Ifelse:
                    case CharstringCommandCodes.Random:
                    case CharstringCommandCodes.Mul:
                    case CharstringCommandCodes.Sqrt:
                    case CharstringCommandCodes.Dup:
                    case CharstringCommandCodes.Exch:
                    case CharstringCommandCodes.Index:
                    case CharstringCommandCodes.Roll:
                    case CharstringCommandCodes.Hflex:
                    case CharstringCommandCodes.Flex:
                    case CharstringCommandCodes.Hflex1:
                    case CharstringCommandCodes.Flex1:
                        f(ope, stack);
                        break;

                    case CharstringCommandCodes.Return:
                    case CharstringCommandCodes.Endchar:
                        return;

                    case CharstringCommandCodes.Callsubr:
                    case CharstringCommandCodes.Callgsubr:
                        f(ope, stack);
                        break;

                    default:
                        throw new();
                }
            }
            else
            {
                stack.Push(CharstringNumber(charstring[i..Math.Min(charstring.Length, 1 + (i += NextNumberBytes(c)))]));
            }
        }
    }

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

    public static byte[] NumberToBytes(int number) =>
        number is >= -107 and <= 107 ? [(byte)(number + 139)] :
        number is >= 108 and <= 1131 ? [(byte)((number - 108) / 256 + 247), (byte)(number - 108)] :
        number is <= -108 and >= -1131 ? [(byte)((-number - 108) / 256 + 251), (byte)(-number - 108)] :
        number is >= -32768 and <= 32767 ? [28, (byte)(number >> 8), (byte)(number & 0xFF)] :
        [255, (byte)(number >> 24), (byte)((number >> 16) & 0xFF), (byte)((number >> 8) & 0xFF), (byte)(number & 0xFF)];

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
