namespace OpenType.Extension;

public readonly struct Offset16
{
    public required ushort Value { get; init; }

    public static implicit operator Offset16(ushort v) => new() { Value = v };
    public static implicit operator Offset16(int v) => new() { Value = (ushort)v };

    public static int SizeOf() => sizeof(ushort);

    public override string ToString() => Value.ToString();
}
