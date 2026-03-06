using System.IO;

namespace OpenType.Tables;

public static class PaintFormat
{
    public static IPaintFormat ReadFrom(Stream stream) => stream.ReadByte() switch
    {
        1 => PaintColrLayers.ReadFrom(stream),
        2 => PaintSolid.ReadFrom(stream),
        3 => PaintVarSolid.ReadFrom(stream),
        4 => PaintLinearGradient.ReadFrom(stream),
        5 => PaintVarLinearGradient.ReadFrom(stream),
        6 => PaintRadialGradient.ReadFrom(stream),
        7 => PaintVarRadialGradient.ReadFrom(stream),
        8 => PaintSweepGradient.ReadFrom(stream),
        9 => PaintVarSweepGradient.ReadFrom(stream),
        _ => throw new()
    };
}
