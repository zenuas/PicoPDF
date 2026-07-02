namespace OpenType.Extension;

public readonly struct Offset24
{
    public required int Value { get; init; }

    public static implicit operator Offset24(int v) => new() { Value = v };

    public static int SizeOf() => 3;

    public override string ToString() => Value.ToString();
}
