namespace OpenType.Extension;

public struct Fixed
{
    public required int Value { get; init; }
    public float FloatValue => Value / 65536F;

    public static implicit operator Fixed(float v) => new() { Value = (int)(v * 65536F) };
    public static implicit operator Fixed(int v) => new() { Value = v };
    public static implicit operator float(Fixed v) => v.FloatValue;
}
