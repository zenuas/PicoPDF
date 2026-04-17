using OpenType.Outline;
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
        public float Shift()
        {
            var value = self[0];
            self.RemoveAt(0);
            return value;
        }
        public float Peek() => self[^1];
    }

    public static void EnumOperands(Span<byte> charstring, List<float> stack, Action<CharstringCommandCodes, List<float>> f)
    {
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

    public static void DefaultOperandActionWithoutCallsubr(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
    {
        switch (ope)
        {
            case CharstringCommandCodes.Vstem:
            case CharstringCommandCodes.Hstem:
            case CharstringCommandCodes.Vstemhm:
            case CharstringCommandCodes.Hstemhm:
                break;

            case CharstringCommandCodes.Vlineto:
            case CharstringCommandCodes.Hlineto:
                {
                    var vline = ope == CharstringCommandCodes.Vlineto;
                    var prev = frame.CurrentPoint;
                    while (stack.Count >= 2)
                    {
                        var value = stack.Shift();
                        frame.CurrentPoint = new(frame.CurrentPoint.X + (!vline ? value : 0), frame.CurrentPoint.Y + (vline ? value : 0));
                        frame.Edges.Add(new Line() { Start = prev, End = frame.CurrentPoint });
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
                        frame.Edges.Add(new Line() { Start = prev, End = frame.CurrentPoint });
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
                        frame.Edges.Add(new BezierCurves()
                        {
                            Start = frame.CurrentPoint,
                            ControlPoint = [cp1, cp2],
                            End = end,
                            ComplementPoint = false,
                        });
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
                        frame.Edges.Add(new BezierCurves()
                        {
                            Start = frame.CurrentPoint,
                            ControlPoint = [cp1, cp2],
                            End = end,
                            ComplementPoint = false,
                        });
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
                            frame.Edges.Add(new BezierCurves()
                            {
                                Start = frame.CurrentPoint,
                                ControlPoint = [cp1, cp2],
                                End = end,
                                ComplementPoint = false,
                            });
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
                            frame.Edges.Add(new BezierCurves()
                            {
                                Start = frame.CurrentPoint,
                                ControlPoint = [cp1, cp2],
                                End = end,
                                ComplementPoint = false,
                            });
                            frame.CurrentPoint = end;
                        }
                        vcurve = !vcurve;
                    }
                    break;
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
                        frame.Edges.Add(new BezierCurves()
                        {
                            Start = frame.CurrentPoint,
                            ControlPoint = [cp1, cp2],
                            End = end,
                            ComplementPoint = false,
                        });
                        frame.CurrentPoint = end;
                    }
                    if (ope == CharstringCommandCodes.Rcurveline)
                    {
                        var prev = frame.CurrentPoint;
                        frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Shift(), frame.CurrentPoint.Y + stack.Shift());
                        frame.Edges.Add(new Line() { Start = prev, End = frame.CurrentPoint });
                    }
                    break;
                }

            case CharstringCommandCodes.Rlinecurve:
                {
                    break;
                }

            case CharstringCommandCodes.Hintmask:
            case CharstringCommandCodes.Cntrmask:
            case CharstringCommandCodes.Shortint:
                break;

            case CharstringCommandCodes.Vmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X, frame.CurrentPoint.Y + stack.Pop()); break;
            case CharstringCommandCodes.Hmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Pop(), frame.CurrentPoint.Y); break;
            case CharstringCommandCodes.Rmoveto: frame.CurrentPoint = new(frame.CurrentPoint.X + stack.Pop(), frame.CurrentPoint.Y + stack.Pop()); break;

            case CharstringCommandCodes.And: stack.Add(stack.Pop() != 0 && stack.Pop() != 0 ? 1 : 0); break;
            case CharstringCommandCodes.Or: stack.Push(stack.Pop() != 0 || stack.Pop() != 0 ? 1 : 0); break;
            case CharstringCommandCodes.Not: stack.Push(stack.Pop() != 0 ? 0 : 1); break;
            case CharstringCommandCodes.Abs: stack.Push(Math.Abs(stack.Pop())); break;
            case CharstringCommandCodes.Add: stack.Push(stack.Pop() + stack.Pop()); break;
            case CharstringCommandCodes.Sub: stack.Push(stack.Pop() - stack.Pop()); break;
            case CharstringCommandCodes.Div: stack.Push(stack.Pop() / stack.Pop()); break;
            case CharstringCommandCodes.Neg: stack.Push(-stack.Pop()); break;
            case CharstringCommandCodes.Eq: stack.Push(stack.Pop() == stack.Pop() ? 1 : 0); break;
            case CharstringCommandCodes.Drop: _ = stack.Pop(); break;

            case CharstringCommandCodes.Put:
                {
                    var value = stack.Pop();
                    var index = (int)stack.Pop();
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
                    var then = stack.Pop();
                    var else_ = stack.Pop();
                    var left = stack.Pop();
                    var right = stack.Pop();
                    stack.Push(left <= right ? then : else_);
                    break;
                }

            case CharstringCommandCodes.Random: stack.Push(Random.Shared.NextSingle()); break;
            case CharstringCommandCodes.Mul: stack.Push(stack.Pop() * stack.Pop()); break;
            case CharstringCommandCodes.Sqrt: stack.Push(MathF.Sqrt(stack.Pop())); break;
            case CharstringCommandCodes.Dup: stack.Push(stack.Peek()); break;

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
                    var n = (int)stack.Pop();
                    var j = (int)stack.Pop();

                    Span<float> values = stackalloc float[n];
                    for (var i = 0; i < n; i++) values[i] = stack[stack.Count - n + i];
                    for (var i = 0; i < n; i++) stack[stack.Count - n + i] = values[(n + i - j) % n];
                    break;
                }

            case CharstringCommandCodes.Hflex:
            case CharstringCommandCodes.Flex:
            case CharstringCommandCodes.Hflex1:
            case CharstringCommandCodes.Flex1:
                break;

            case CharstringCommandCodes.Return:
            case CharstringCommandCodes.Endchar:
                break;

            case CharstringCommandCodes.Callsubr:
            case CharstringCommandCodes.Callgsubr:
            default:
                throw new();
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
