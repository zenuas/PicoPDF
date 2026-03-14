using Mina.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public static class PaintFormat
{
    public static IPaintFormat ReadFrom(Stream stream, long position, Dictionary<long, IPaintFormat> paintCache) => paintCache.TryGetValue(position, out var paint) ? paint : paintCache[position] = ReadFrom(stream.SeekTo(position), paintCache);

    public static IPaintFormat ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache) => stream.ReadByte() switch
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
        10 => PaintGlyph.ReadFrom(stream, paintCache),
        11 => PaintColrGlyph.ReadFrom(stream),
        12 => PaintTransform.ReadFrom(stream, paintCache),
        13 => PaintVarTransform.ReadFrom(stream, paintCache),
        14 => PaintTranslate.ReadFrom(stream, paintCache),
        15 => PaintVarTranslate.ReadFrom(stream, paintCache),
        16 => PaintScale.ReadFrom(stream, paintCache),
        17 => PaintVarScale.ReadFrom(stream, paintCache),
        18 => PaintScaleAroundCenter.ReadFrom(stream, paintCache),
        19 => PaintVarScaleAroundCenter.ReadFrom(stream, paintCache),
        20 => PaintScaleUniform.ReadFrom(stream, paintCache),
        21 => PaintVarScaleUniform.ReadFrom(stream, paintCache),
        22 => PaintScaleUniformAroundCenter.ReadFrom(stream, paintCache),
        23 => PaintVarScaleUniformAroundCenter.ReadFrom(stream, paintCache),
        24 => PaintRotate.ReadFrom(stream, paintCache),
        25 => PaintVarRotate.ReadFrom(stream, paintCache),
        26 => PaintRotateAroundCenter.ReadFrom(stream, paintCache),
        27 => PaintVarRotateAroundCenter.ReadFrom(stream, paintCache),
        28 => PaintSkew.ReadFrom(stream, paintCache),
        29 => PaintVarSkew.ReadFrom(stream, paintCache),
        30 => PaintSkewAroundCenter.ReadFrom(stream, paintCache),
        31 => PaintVarSkewAroundCenter.ReadFrom(stream, paintCache),
        32 => PaintComposite.ReadFrom(stream, paintCache),
        var format => new PaintN { Format = (byte)format },
    };
}
