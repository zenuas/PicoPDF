namespace OpenType.Tables.Colr;

public interface IAffine2x3
{
    public uint XX { get; init; }
    public uint YX { get; init; }
    public uint XY { get; init; }
    public uint YY { get; init; }
    public uint DX { get; init; }
    public uint DY { get; init; }
}
