using Mina.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public static class PaintFormat
{
    public static IPaintFormat ReadFrom(Stream stream, long position,
        Dictionary<long, IPaintFormat> paintCache,
        Dictionary<long, IColorLine> colorLineCache,
        Dictionary<long, IAffine2x3> affineCache) => paintCache.TryGetValue(position, out var paint) ? paint : paintCache[position] = ReadFrom(stream.SeekTo(position), paintCache, colorLineCache, affineCache);

    public static IPaintFormat ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache, Dictionary<long, IAffine2x3> affineCache) => stream.ReadByte() switch
    {
        1 => PaintColrLayers.ReadFrom(stream),
        2 => PaintSolid.ReadFrom(stream),
        3 => PaintVarSolid.ReadFrom(stream),
        4 => PaintLinearGradient.ReadFrom(stream, colorLineCache),
        5 => PaintVarLinearGradient.ReadFrom(stream, colorLineCache),
        6 => PaintRadialGradient.ReadFrom(stream, colorLineCache),
        7 => PaintVarRadialGradient.ReadFrom(stream, colorLineCache),
        8 => PaintSweepGradient.ReadFrom(stream, colorLineCache),
        9 => PaintVarSweepGradient.ReadFrom(stream, colorLineCache),
        10 => PaintGlyph.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        11 => PaintColrGlyph.ReadFrom(stream),
        12 => PaintTransform.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        13 => PaintVarTransform.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        14 => PaintTranslate.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        15 => PaintVarTranslate.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        16 => PaintScale.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        17 => PaintVarScale.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        18 => PaintScaleAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        19 => PaintVarScaleAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        20 => PaintScaleUniform.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        21 => PaintVarScaleUniform.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        22 => PaintScaleUniformAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        23 => PaintVarScaleUniformAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        24 => PaintRotate.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        25 => PaintVarRotate.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        26 => PaintRotateAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        27 => PaintVarRotateAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        28 => PaintSkew.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        29 => PaintVarSkew.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        30 => PaintSkewAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        31 => PaintVarSkewAroundCenter.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        32 => PaintComposite.ReadFrom(stream, paintCache, colorLineCache, affineCache),
        var format => new PaintN { Format = (byte)format },
    };
}
