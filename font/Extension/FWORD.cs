namespace OpenType.Extension;

public readonly struct FWORD
{
    public required short Value { get; init; }

    public static implicit operator FWORD(short v) => new() { Value = v };
    public static implicit operator FWORD(int v) => new() { Value = (short)v };

    public static int SizeOf() => sizeof(short);

    public override string ToString() => Value.ToString();
}
