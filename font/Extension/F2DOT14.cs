namespace OpenType.Extension;

public readonly struct F2DOT14
{
    public required short Value { get; init; }
    public float FloatValue => Value / 16384F;

    public static implicit operator F2DOT14(float v) => new() { Value = (short)(v * 16384F) };
    public static implicit operator F2DOT14(short v) => new() { Value = v };
    public static implicit operator float(F2DOT14 v) => v.FloatValue;

    public override string ToString() => FloatValue.ToString();
}
