using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

namespace OpenType.Tables.PostScript;

public static class Subroutine
{
    extension(List<float> self)
    {
        public void Push(float value) => self.Add(value);

        public float Pop()
        {
            var value = self[^1];
            self.RemoveAt(self.Count - 1);
            return value;
        }

        public float Pop2()
        {
            var value = self[^2];
            self.RemoveAt(self.Count - 2);
            return value;
        }

        public float Shift()
        {
            var value = self[0];
            self.RemoveAt(0);
            return value;
        }

        public float Peek() => self[^1];
    }

    public static void EnumOperands(Span<byte> charstring, List<float> stack, SubroutineFrame frame, Action<CharstringCommandCodes, List<float>, SubroutineFrame> f)
    {
        // w? {hs* vs* cm* hm* mt subpath}? {mt subpath}* endchar
        // Where:
        //   w = width
        //   hs = hstem or hstemhm command
        //   vs = vstem or vstemhm command
        //   cm = cntrmask operator
        //   hm = hintmask operator
        //   mt = moveto (i.e. any of the moveto) operators
        //   subpath = refers to the construction of a subpath (one complete closed contour),
        //             which may include hintmask operators where appropriate.
        for (var i = 0; i < charstring.Length; i++)
        {
            var c = charstring[i];
            if (c < 32)
            {
                var ope = (CharstringCommandCodes)(c == (int)CharstringCommandCodes.Escape ? ((int)CharstringCommandCodes.Escape * 100) + charstring[++i] : c);
                switch (ope)
                {
                    case CharstringCommandCodes.Shortint:
                        stack.Push((short)(charstring[++i] << 8 | charstring[++i]));
                        f(ope, stack, frame);
                        break;

                    case CharstringCommandCodes.Hstem:
                    case CharstringCommandCodes.Vstem:
                    case CharstringCommandCodes.Hstemhm:
                    case CharstringCommandCodes.Vstemhm:
                        if (frame.Width is null) f(CharstringCommandCodes.Width, stack.Count % 2 == 1 ? [stack.Shift()] : [], frame);
                        f(ope, stack, frame);
                        stack.Clear();
                        break;

                    case CharstringCommandCodes.Rmoveto:
                        if (frame.Width is null) f(CharstringCommandCodes.Width, stack.Count > 2 ? [stack.Shift()] : [], frame);
                        f(ope, stack, frame);
                        stack.Clear();
                        break;

                    case CharstringCommandCodes.Vmoveto:
                    case CharstringCommandCodes.Hmoveto:
                        if (frame.Width is null) f(CharstringCommandCodes.Width, stack.Count > 1 ? [stack.Shift()] : [], frame);
                        f(ope, stack, frame);
                        stack.Clear();
                        break;

                    case CharstringCommandCodes.Rlineto:
                    case CharstringCommandCodes.Hlineto:
                    case CharstringCommandCodes.Vlineto:
                    case CharstringCommandCodes.Rrcurveto:
                    case CharstringCommandCodes.Rcurveline:
                    case CharstringCommandCodes.Rlinecurve:
                    case CharstringCommandCodes.Vvcurveto:
                    case CharstringCommandCodes.Hhcurveto:
                    case CharstringCommandCodes.Vhcurveto:
                    case CharstringCommandCodes.Hvcurveto:
                    case CharstringCommandCodes.Hflex:
                    case CharstringCommandCodes.Hflex1:
                    case CharstringCommandCodes.Flex:
                    case CharstringCommandCodes.Flex1:
                        f(ope, stack, frame);
                        stack.Clear();
                        break;

                    case CharstringCommandCodes.Hintmask:
                    case CharstringCommandCodes.Cntrmask:
                        {
                            if (frame.Width is null) f(CharstringCommandCodes.Width, stack.Count % 2 == 1 ? [stack.Shift()] : [], frame);

                            // If hstem and vstem hints are both declared at the beginning of a charstring,
                            // and this sequence is followed directly by the hintmask or cntrmask operators,
                            // the vstem hint operator need not be included. 
                            if (stack.Count > 0) f(CharstringCommandCodes.Vstem, stack, frame);
                            stack.Clear();
                            var step = (frame.StemPairCount + 7) / 8;
                            f(ope, [.. charstring[(i + 1)..(i + step + 1)]], frame);
                            i += step;
                            break;
                        }

                    case CharstringCommandCodes.Callsubr:
                    case CharstringCommandCodes.Callgsubr:
                        // The charstring itself may end with a call(g)subr; the subroutine must then end with an endchar operator.
                        f(ope, stack, frame);
                        if (frame.IsEndchar) return;
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
                        f(ope, stack, frame);
                        break;

                    case CharstringCommandCodes.Return:
                        f(ope, stack, frame);
                        return;

                    case CharstringCommandCodes.Endchar:
                        // A character that does not have a path (e.g. a space character) may consist of an endchar operator preceded only by a width value.
                        // Although the width must be specified in the font, it may be specified as the defaultWidthX in the CFF data, in which case it should not be specified in the charstring.
                        // Also, it may appear in the charstring as the difference from nominalWidthX. Thus the smallest legal charstring consists of a single endchar operator.
                        if (frame.Width is null) f(CharstringCommandCodes.Width, stack.Count > 0 ? [stack.Shift()] : [], frame);
                        f(ope, stack, frame);
                        return;

                    default:
                        throw new();
                }
            }
            else
            {
                stack.Push(CharstringNumber(charstring[i..Math.Min(charstring.Length, 1 + (i += NextNumberBytes(c)))]));
            }
        }
        f(CharstringCommandCodes.Return, stack, frame);
    }

    public static void DefaultOperandAction(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
    {
        switch (ope)
        {
            case CharstringCommandCodes.Vstem:
            case CharstringCommandCodes.Hstem:
            case CharstringCommandCodes.Vstemhm:
            case CharstringCommandCodes.Hstemhm:
                frame.StemPairCount += stack.Count / 2;
                break;

            case CharstringCommandCodes.Vlineto:
            case CharstringCommandCodes.Hlineto:
                {
                    var vline = ope == CharstringCommandCodes.Vlineto;
                    var prev = frame.CurrentPoint;
                    while (stack.Count > 0)
                    {
                        var value = stack.Shift();
                        frame.CurrentPoint = new(frame.CurrentPoint.X + (!vline ? value : 0), frame.CurrentPoint.Y + (vline ? value : 0));
                        frame.AddLine([prev, frame.CurrentPoint]);
                        prev = frame.CurrentPoint;
                        vline = !vline;
                    }
                    break;
                }

            case CharstringCommandCodes.Rlineto:
                {
                    var prev = frame.CurrentPoint;
                    while (stack.Count >= 2)
                    {
                        frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Shift(), frame.CurrentPoint.Y + stack.Shift());
                        frame.AddLine([prev, frame.CurrentPoint]);
                        prev = frame.CurrentPoint;
                    }
                    break;
                }

            case CharstringCommandCodes.Vvcurveto:
                {
                    if (stack.Count % 2 != 0) frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Shift(), frame.CurrentPoint.Y);
                    while (stack.Count >= 4)
                    {
                        var dya = stack.Shift();
                        var dxb = stack.Shift();
                        var dyb = stack.Shift();
                        var dyc = stack.Shift();

                        var cp1 = new Vector2(frame.CurrentPoint.X, frame.CurrentPoint.Y + dya);
                        var cp2 = new Vector2(cp1.X + dxb, cp1.Y + dyb);
                        var end = new Vector2(cp2.X, cp2.Y + dyc);
                        frame.AddLine([frame.CurrentPoint, cp1, cp2, end]);
                        frame.CurrentPoint = end;
                    }
                    break;
                }

            case CharstringCommandCodes.Hhcurveto:
                {
                    if (stack.Count % 2 != 0) frame.CurrentPoint = new(frame.CurrentPoint.X, frame.CurrentPoint.Y + stack.Shift());
                    while (stack.Count >= 4)
                    {
                        var dxa = stack.Shift();
                        var dxb = stack.Shift();
                        var dyb = stack.Shift();
                        var dxc = stack.Shift();

                        var cp1 = new Vector2(frame.CurrentPoint.X + dxa, frame.CurrentPoint.Y);
                        var cp2 = new Vector2(cp1.X + dxb, cp1.Y + dyb);
                        var end = new Vector2(cp2.X + dxc, cp2.Y);
                        frame.AddLine([frame.CurrentPoint, cp1, cp2, end]);
                        frame.CurrentPoint = end;
                    }
                    break;
                }

            case CharstringCommandCodes.Vhcurveto:
            case CharstringCommandCodes.Hvcurveto:
                {
                    var vcurve = ope == CharstringCommandCodes.Vhcurveto;
                    while (stack.Count >= 4)
                    {
                        if (vcurve)
                        {
                            var dy1 = stack.Shift();
                            var dx2 = stack.Shift();
                            var dy2 = stack.Shift();
                            var dx3 = stack.Shift();
                            var dyf = stack.Count == 1 ? stack.Shift() : 0;

                            var cp1 = new Vector2(frame.CurrentPoint.X, frame.CurrentPoint.Y + dy1);
                            var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                            var end = new Vector2(cp2.X + dx3, cp2.Y + dyf);
                            frame.AddLine([frame.CurrentPoint, cp1, cp2, end]);
                            frame.CurrentPoint = end;
                        }
                        else
                        {
                            var dx1 = stack.Shift();
                            var dx2 = stack.Shift();
                            var dy2 = stack.Shift();
                            var dy3 = stack.Shift();
                            var dxf = stack.Count == 1 ? stack.Shift() : 0;

                            var cp1 = new Vector2(frame.CurrentPoint.X + dx1, frame.CurrentPoint.Y);
                            var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                            var end = new Vector2(cp2.X + dxf, cp2.Y + dy3);
                            frame.AddLine([frame.CurrentPoint, cp1, cp2, end]);
                            frame.CurrentPoint = end;
                        }
                        vcurve = !vcurve;
                    }
                    break;
                }

            case CharstringCommandCodes.Rlinecurve:
                {
                    var prev = frame.CurrentPoint;
                    while (stack.Count >= 8)
                    {
                        frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Shift(), frame.CurrentPoint.Y + stack.Shift());
                        frame.AddLine([prev, frame.CurrentPoint]);
                    }
                    goto case CharstringCommandCodes.Rrcurveto;
                }

            case CharstringCommandCodes.Rrcurveto:
            case CharstringCommandCodes.Rcurveline:
                {
                    while (stack.Count >= 6)
                    {
                        var dxa = stack.Shift();
                        var dya = stack.Shift();
                        var dxb = stack.Shift();
                        var dyb = stack.Shift();
                        var dxc = stack.Shift();
                        var dyc = stack.Shift();

                        var cp1 = new Vector2(frame.CurrentPoint.X + dxa, frame.CurrentPoint.Y + dya);
                        var cp2 = new Vector2(cp1.X + dxb, cp1.Y + dyb);
                        var end = new Vector2(cp2.X + dxc, cp2.Y + dyc);
                        frame.AddLine([frame.CurrentPoint, cp1, cp2, end]);
                        frame.CurrentPoint = end;
                    }
                    if (ope == CharstringCommandCodes.Rcurveline)
                    {
                        var prev = frame.CurrentPoint;
                        frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Shift(), frame.CurrentPoint.Y + stack.Shift());
                        frame.AddLine([prev, frame.CurrentPoint]);
                    }
                    break;
                }

            case CharstringCommandCodes.Vmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X, frame.CurrentPoint.Y + stack.Pop()); break;
            case CharstringCommandCodes.Hmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Pop(), frame.CurrentPoint.Y); break;
            case CharstringCommandCodes.Rmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Pop2(), frame.CurrentPoint.Y + stack.Pop()); break;

            case CharstringCommandCodes.And: stack.Add(stack.Pop() != 0 && stack.Pop() != 0 ? 1 : 0); break;
            case CharstringCommandCodes.Or: stack.Push(stack.Pop() != 0 || stack.Pop() != 0 ? 1 : 0); break;
            case CharstringCommandCodes.Not: stack.Push(stack.Pop() != 0 ? 0 : 1); break;
            case CharstringCommandCodes.Abs: stack.Push(Math.Abs(stack.Pop())); break;
            case CharstringCommandCodes.Add: stack.Push(stack.Pop() + stack.Pop()); break;
            case CharstringCommandCodes.Sub: stack.Push(stack.Pop2() - stack.Pop()); break;
            case CharstringCommandCodes.Mul: stack.Push(stack.Pop() * stack.Pop()); break;
            case CharstringCommandCodes.Div: stack.Push(stack.Pop2() / stack.Pop()); break;
            case CharstringCommandCodes.Neg: stack.Push(-stack.Pop()); break;
            case CharstringCommandCodes.Eq: stack.Push(stack.Pop() == stack.Pop() ? 1 : 0); break;
            case CharstringCommandCodes.Drop: _ = stack.Pop(); break;
            case CharstringCommandCodes.Random: stack.Push(Random.Shared.NextSingle()); break;
            case CharstringCommandCodes.Sqrt: stack.Push(MathF.Sqrt(stack.Pop())); break;
            case CharstringCommandCodes.Dup: stack.Push(stack.Peek()); break;

            case CharstringCommandCodes.Put:
                {
                    var index = (int)stack.Pop();
                    var value = stack.Pop();
                    frame.TransientArray[index] = value;
                    break;
                }

            case CharstringCommandCodes.Get:
                {
                    var index = (int)stack.Pop();
                    stack.Push(frame.TransientArray.TryGetValue(index, out var value) ? value : 0);
                    _ = frame.TransientArray.Remove(index);
                    break;
                }

            case CharstringCommandCodes.Ifelse:
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    var else_ = stack.Pop();
                    var then = stack.Pop();
                    stack.Push(left <= right ? then : else_);
                    break;
                }

            case CharstringCommandCodes.Exch:
                {
                    var value1 = stack.Pop();
                    var value2 = stack.Pop();
                    stack.Push(value1);
                    stack.Push(value2);
                    break;
                }

            case CharstringCommandCodes.Index:
                {
                    var index = (int)stack.Pop();
                    if (index < 0)
                    {
                        stack.Push(stack.Peek());
                    }
                    else
                    {
                        stack.Push(stack[^(index + 1)]);
                    }
                    break;
                }

            case CharstringCommandCodes.Roll:
                {
                    var j = (int)stack.Pop();
                    var n = (int)stack.Pop();

                    Span<float> values = stackalloc float[n];
                    for (var i = 0; i < n; i++) values[i] = stack[stack.Count - n + i];
                    for (var i = 0; i < n; i++) stack[stack.Count - n + i] = values[(n + i - j) % n];
                    break;
                }

            case CharstringCommandCodes.Hflex:
                {
                    var dx1 = stack.Shift();
                    var dx2 = stack.Shift();
                    var dy2 = stack.Shift();
                    var dx3 = stack.Shift();
                    var dx4 = stack.Shift();
                    var dx5 = stack.Shift();
                    var dx6 = stack.Shift();

                    var cp1 = new Vector2(frame.CurrentPoint.X + dx1, frame.CurrentPoint.Y);
                    var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                    var cp3 = new Vector2(cp2.X + dx3, cp2.Y);
                    frame.AddLine([frame.CurrentPoint, cp1, cp2, cp3]);

                    var cp4 = new Vector2(cp3.X + dx4, cp3.Y);
                    var cp5 = new Vector2(cp4.X + dx5, cp4.Y);
                    var cp6 = new Vector2(cp5.X + dx6, cp5.Y);
                    frame.AddLine([cp3, cp4, cp5, cp6]);
                    frame.CurrentPoint = cp6;
                    break;
                }

            case CharstringCommandCodes.Hflex1:
                {
                    var dx1 = stack.Shift();
                    var dy1 = stack.Shift();
                    var dx2 = stack.Shift();
                    var dy2 = stack.Shift();
                    var dx3 = stack.Shift();
                    var dx4 = stack.Shift();
                    var dx5 = stack.Shift();
                    var dy5 = stack.Shift();
                    var dx6 = stack.Shift();

                    var cp1 = new Vector2(frame.CurrentPoint.X + dx1, frame.CurrentPoint.Y + dy1);
                    var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                    var cp3 = new Vector2(cp2.X + dx3, cp2.Y);
                    frame.AddLine([frame.CurrentPoint, cp1, cp2, cp3]);

                    var cp4 = new Vector2(cp3.X + dx4, cp3.Y);
                    var cp5 = new Vector2(cp4.X + dx5, cp4.Y + dy5);
                    var cp6 = new Vector2(cp5.X + dx6, cp5.Y);
                    frame.AddLine([cp3, cp4, cp5, cp6]);
                    frame.CurrentPoint = cp6;
                    break;
                }

            case CharstringCommandCodes.Flex:
                {
                    var dx1 = stack.Shift();
                    var dy1 = stack.Shift();
                    var dx2 = stack.Shift();
                    var dy2 = stack.Shift();
                    var dx3 = stack.Shift();
                    var dy3 = stack.Shift();
                    var dx4 = stack.Shift();
                    var dy4 = stack.Shift();
                    var dx5 = stack.Shift();
                    var dy5 = stack.Shift();
                    var dx6 = stack.Shift();
                    var dy6 = stack.Shift();
                    _ = stack.Shift(); // Ignore flex depth.

                    var cp1 = new Vector2(frame.CurrentPoint.X + dx1, frame.CurrentPoint.Y + dy1);
                    var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                    var cp3 = new Vector2(cp2.X + dx3, cp2.Y + dy3);
                    frame.AddLine([frame.CurrentPoint, cp1, cp2, cp3]);

                    var cp4 = new Vector2(cp3.X + dx4, cp3.Y + dy4);
                    var cp5 = new Vector2(cp4.X + dx5, cp4.Y + dy5);
                    var cp6 = new Vector2(cp5.X + dx6, cp5.Y + dy6);
                    frame.AddLine([cp3, cp4, cp5, cp6]);
                    frame.CurrentPoint = cp6;
                    break;
                }

            case CharstringCommandCodes.Flex1:
                {
                    var dx1 = stack.Shift();
                    var dy1 = stack.Shift();
                    var dx2 = stack.Shift();
                    var dy2 = stack.Shift();
                    var dx3 = stack.Shift();
                    var dy3 = stack.Shift();
                    var dx4 = stack.Shift();
                    var dy4 = stack.Shift();
                    var dx5 = stack.Shift();
                    var dy5 = stack.Shift();
                    var d6 = stack.Shift();

                    var cp1 = new Vector2(frame.CurrentPoint.X + dx1, frame.CurrentPoint.Y + dy1);
                    var cp2 = new Vector2(cp1.X + dx2, cp1.Y + dy2);
                    var cp3 = new Vector2(cp2.X + dx3, cp2.Y + dy3);
                    frame.AddLine([frame.CurrentPoint, cp1, cp2, cp3]);

                    var cp4 = new Vector2(cp3.X + dx4, cp3.Y + dy4);
                    var cp5 = new Vector2(cp4.X + dx5, cp4.Y + dy5);
                    var cp6 = MathF.Abs(cp5.X - frame.CurrentPoint.X) > Math.Abs(cp5.Y - frame.CurrentPoint.Y) ? new Vector2(cp5.X + d6, cp5.Y) : new Vector2(cp5.X, cp5.Y + d6);
                    frame.AddLine([cp3, cp4, cp5, cp6]);
                    frame.CurrentPoint = cp6;
                    break;
                }

            case CharstringCommandCodes.Endchar:
                frame.IsEndchar = true;
                break;

            case CharstringCommandCodes.Return:
            case CharstringCommandCodes.Callsubr:
            case CharstringCommandCodes.Callgsubr:
            case CharstringCommandCodes.Shortint:
            case CharstringCommandCodes.Hintmask:
            case CharstringCommandCodes.Cntrmask:
                break;

            case CharstringCommandCodes.Width:
                frame.Width ??= 0;
                break;

            default:
                throw new();
        }
    }

    public static void EnumSubroutines(Span<byte> charstring, byte[][] local_subr, byte[][] global_subr, Func<bool, int, bool> f)
    {
        var local_bias = GetSubroutineBias(local_subr.Length);
        var global_bias = GetSubroutineBias(global_subr.Length);

        void OperandAction(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
        {
            switch (ope)
            {
                case CharstringCommandCodes.Callsubr:
                    {
                        var index = (int)stack.Pop() + local_bias;
                        Debug.Assert(index >= 0 && index < local_subr.Length);
                        if (index >= 0 && index < local_subr.Length && f(false, index)) Subroutine.EnumOperands(local_subr[index], stack, frame, OperandAction);
                        break;
                    }

                case CharstringCommandCodes.Callgsubr:
                    {
                        var index = (int)stack.Pop() + global_bias;
                        Debug.Assert(index >= 0 && index < global_subr.Length);
                        if (index >= 0 && index < global_subr.Length && f(true, index)) Subroutine.EnumOperands(global_subr[index], stack, frame, OperandAction);
                        break;
                    }

                default:
                    Subroutine.DefaultOperandAction(ope, stack, frame);
                    break;
            }
        }

        Subroutine.EnumOperands(charstring, [], new SubroutineFrame(), OperandAction);

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

    public static float CharstringNumber(Span<byte> charstring) =>
        charstring.Length == 1 && charstring[0] is >= 32 and <= 246 ? charstring[0] - 139 :
        charstring.Length == 2 && charstring[0] is >= 247 and <= 250 ? (charstring[0] - 247) * 256 + charstring[1] + 108 :
        charstring.Length == 2 && charstring[0] is >= 251 and <= 254 ? -((charstring[0] - 251) * 256) - charstring[1] - 108 :
        charstring.Length == 5 && charstring[0] == 255 ? ((short)(charstring[1] << 8 | charstring[2])) + ((short)(charstring[3] << 8 | charstring[4])) / 65536f :
        0;

    public static int NextNumberBytes(byte c) =>
        c == 255 ? 4 :
        c <= 246 ? 0 :
        1;
}
