namespace OpenType.Extension;

public readonly struct Offset32
{
    public required uint Value { get; init; }

    public static implicit operator Offset32(int v) => new() { Value = (uint)v };
    public static implicit operator Offset32(uint v) => new() { Value = v };

    public static int SizeOf() => sizeof(uint);

    public override string ToString() => Value.ToString();
}
