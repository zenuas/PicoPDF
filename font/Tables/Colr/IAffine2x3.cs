using OpenType.Extension;

namespace OpenType.Tables.Colr;

public interface IAffine2x3
{
    public Fixed XX { get; init; }
    public Fixed YX { get; init; }
    public Fixed XY { get; init; }
    public Fixed YY { get; init; }
    public Fixed DX { get; init; }
    public Fixed DY { get; init; }
}
