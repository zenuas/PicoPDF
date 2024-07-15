using Mina.Extension;

namespace PicoPDF.OpenType.Tables.PostScript;

public class IntOrDouble
{
    public required object Value { get; init; }

    public static implicit operator IntOrDouble(byte x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(sbyte x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(short x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(ushort x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(int x) => new() { Value = x };
    public static implicit operator IntOrDouble(uint x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(long x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(ulong x) => new() { Value = (int)x };
    public static implicit operator IntOrDouble(float x) => new() { Value = (double)x };
    public static implicit operator IntOrDouble(double x) => new() { Value = x };

    public int ToInt() => SafeConvert.ToInt(Value);

    public double ToDouble() => SafeConvert.ToDouble(Value);

    public bool IsInt() => Value is int;

    public bool IsDouble() => Value is double;

    public override string ToString() => IsInt() ? $"{ToInt()}" : $"{ToDouble()}";
}
