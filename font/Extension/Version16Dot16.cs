namespace OpenType.Extension;

public readonly struct Version16Dot16
{
    public required uint Value { get; init; }

    public static implicit operator Version16Dot16(int v) => new() { Value = (uint)v };
    public static implicit operator Version16Dot16(uint v) => new() { Value = v };

    public static int SizeOf() => sizeof(uint);

    public int MajorVersion => (int)(Value >> 16);
    public int MinorVersion => (int)(Value & 0xFFFF);

    public override string ToString() => $"{MajorVersion}.{MinorVersion:x4}";
}
