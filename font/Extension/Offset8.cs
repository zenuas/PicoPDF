namespace OpenType.Extension;

public readonly struct Offset8
{
    public required byte Value { get; init; }

    public static implicit operator Offset8(byte v) => new() { Value = v };
    public static implicit operator Offset8(int v) => new() { Value = (byte)v };

    public static int SizeOf() => sizeof(byte);

    public override string ToString() => Value.ToString();
}
