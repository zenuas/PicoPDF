namespace OpenType.Extension;

public readonly struct UFWORD
{
    public required ushort Value { get; init; }

    public static implicit operator UFWORD(ushort v) => new() { Value = v };
    public static implicit operator UFWORD(int v) => new() { Value = (ushort)v };

    public static int SizeOf() => sizeof(ushort);

    public override string ToString() => Value.ToString();
}
