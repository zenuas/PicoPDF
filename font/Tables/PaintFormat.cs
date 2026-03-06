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
        10 => PaintGlyph.ReadFrom(stream),
        11 => PaintColrGlyph.ReadFrom(stream),
        12 => PaintTransform.ReadFrom(stream),
        13 => PaintVarTransform.ReadFrom(stream),
        14 => PaintTranslate.ReadFrom(stream),
        15 => PaintVarTranslate.ReadFrom(stream),
        16 => PaintScale.ReadFrom(stream),
        17 => PaintVarScale.ReadFrom(stream),
        18 => PaintScaleAroundCenter.ReadFrom(stream),
        19 => PaintVarScaleAroundCenter.ReadFrom(stream),
        _ => throw new()
    };
}
