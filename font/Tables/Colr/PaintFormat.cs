using System.IO;

namespace OpenType.Tables.Colr;

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
        20 => PaintScaleUniform.ReadFrom(stream),
        21 => PaintVarScaleUniform.ReadFrom(stream),
        22 => PaintScaleUniformAroundCenter.ReadFrom(stream),
        23 => PaintVarScaleUniformAroundCenter.ReadFrom(stream),
        24 => PaintRotate.ReadFrom(stream),
        25 => PaintVarRotate.ReadFrom(stream),
        26 => PaintRotateAroundCenter.ReadFrom(stream),
        27 => PaintVarRotateAroundCenter.ReadFrom(stream),
        28 => PaintSkew.ReadFrom(stream),
        29 => PaintVarSkew.ReadFrom(stream),
        30 => PaintSkewAroundCenter.ReadFrom(stream),
        31 => PaintVarSkewAroundCenter.ReadFrom(stream),
        32 => PaintComposite.ReadFrom(stream),
        _ => throw new()
    };
}
