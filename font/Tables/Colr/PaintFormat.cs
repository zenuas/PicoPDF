using Mina.Extension;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables.Colr;

public static class PaintFormat
{
    public static IPaintFormat ReadFrom(Stream stream, long position, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache) => paintCache.TryGetValue(position, out var paint) ? paint : paintCache[position] = ReadFrom(stream.SeekTo(position), paintCache, colorLineCache);

    public static IPaintFormat ReadFrom(Stream stream, Dictionary<long, IPaintFormat> paintCache, Dictionary<long, IColorLine> colorLineCache) => stream.ReadByte() switch
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
        10 => PaintGlyph.ReadFrom(stream, paintCache, colorLineCache),
        11 => PaintColrGlyph.ReadFrom(stream),
        12 => PaintTransform.ReadFrom(stream, paintCache, colorLineCache),
        13 => PaintVarTransform.ReadFrom(stream, paintCache, colorLineCache),
        14 => PaintTranslate.ReadFrom(stream, paintCache, colorLineCache),
        15 => PaintVarTranslate.ReadFrom(stream, paintCache, colorLineCache),
        16 => PaintScale.ReadFrom(stream, paintCache, colorLineCache),
        17 => PaintVarScale.ReadFrom(stream, paintCache, colorLineCache),
        18 => PaintScaleAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        19 => PaintVarScaleAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        20 => PaintScaleUniform.ReadFrom(stream, paintCache, colorLineCache),
        21 => PaintVarScaleUniform.ReadFrom(stream, paintCache, colorLineCache),
        22 => PaintScaleUniformAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        23 => PaintVarScaleUniformAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        24 => PaintRotate.ReadFrom(stream, paintCache, colorLineCache),
        25 => PaintVarRotate.ReadFrom(stream, paintCache, colorLineCache),
        26 => PaintRotateAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        27 => PaintVarRotateAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        28 => PaintSkew.ReadFrom(stream, paintCache, colorLineCache),
        29 => PaintVarSkew.ReadFrom(stream, paintCache, colorLineCache),
        30 => PaintSkewAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        31 => PaintVarSkewAroundCenter.ReadFrom(stream, paintCache, colorLineCache),
        32 => PaintComposite.ReadFrom(stream, paintCache, colorLineCache),
        var format => new PaintN { Format = (byte)format },
    };
}
