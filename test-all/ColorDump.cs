using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Tables;
using OpenType.Tables.Colr;
using System;
using System.Linq;

namespace PicoPDF.TestAll;

public class ColorDump : FontRegisterCommand
{
    [CommandOption("font")]
    public string Font { get; init; } = "Segoe UI Emoji";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);

        if (font.Color is null) return;

        foreach (var cid in args.Select(x => x.ToUtf32CharArray()).Flatten())
        {
            var gid = font.CharToGID(cid);
            if (gid == 0) continue;

            Console.WriteLine($"Char: {char.ConvertFromUtf32(cid)}  (U+{(cid <= 0xFFFF ? cid.ToString("X4") : cid.ToString("X"))})");
            Console.WriteLine($"CID: {gid}");
            EnumColor(font, gid);
        }
    }

    public static void EnumColor(IOpenTypeFont font, uint gid)
    {
        foreach (var record in font.Color!.BaseGlyphRecords.Where(x => x.GlyphID == gid))
        {
            foreach (var layer in font.Color.LayerRecords[record.FirstLayerIndex..(record.FirstLayerIndex + record.NumberOfLayers)])
            {
                EnumColor(font, layer.GlyphID);
            }
        }

        for (var i = 0; i < font.Color.BaseGlyphListRecord?.BaseGlyphPaintRecord.Length; i++)
        {
            if (font.Color.BaseGlyphListRecord.BaseGlyphPaintRecord[i].GlyphID == gid)
            {
                EnumPaint(font.Color.BaseGlyphListRecord.Paints[i], font.Color, 0);
            }
        }
    }

    public static void EnumPaint(IPaintFormat paint, ColorTable colr, int indent)
    {
        DumpPaint(paint, colr, indent);

        if (paint is PaintColrLayers paintColrLayers)
        {
            foreach (var layer in colr.LayerListRecord!.Paints[(int)paintColrLayers.FirstLayerIndex..((int)paintColrLayers.FirstLayerIndex + paintColrLayers.NumberOfLayers)])
            {
                EnumPaint(layer, colr, indent + 1);
            }
        }
        else if (paint is PaintComposite paintComposite)
        {
            EnumPaint(paintComposite.SourcePaint, colr, indent + 1);
            EnumPaint(paintComposite.BackdropPaint, colr, indent + 1);
        }
        else
        {
            if (paint is IHavePaint subpaint)
            {
                EnumPaint(subpaint.Paint, colr, indent + 1);
            }
        }
    }

    public static void DumpPaint(IPaintFormat paint, ColorTable colr, int indent)
    {
        Console.WriteLine($"{new string(' ', indent * 2)}Format: {paint.Format} ({paint.GetType().Name})");
        var sp = new string(' ', (indent + 1) * 2);

        switch (paint)
        {
            case PaintColrLayers p:
                Console.WriteLine($"{sp}NumberOfLayers: {p.NumberOfLayers}");
                Console.WriteLine($"{sp}FirstLayerIndex: {p.FirstLayerIndex}");
                break;

            case PaintSolid p:
                Console.WriteLine($"{sp}PaletteIndex: {p.PaletteIndex}");
                Console.WriteLine($"{sp}Alpha: {(float)p.Alpha}");
                break;

            case PaintVarSolid p:
                Console.WriteLine($"{sp}PaletteIndex: {p.PaletteIndex}");
                Console.WriteLine($"{sp}Alpha: {(float)p.Alpha}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintLinearGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}X0: {p.X0}");
                Console.WriteLine($"{sp}Y0: {p.Y0}");
                Console.WriteLine($"{sp}X1: {p.X1}");
                Console.WriteLine($"{sp}Y1: {p.Y1}");
                Console.WriteLine($"{sp}X2: {p.X2}");
                Console.WriteLine($"{sp}Y2: {p.Y2}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                break;

            case PaintVarLinearGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}X0: {p.X0}");
                Console.WriteLine($"{sp}Y0: {p.Y0}");
                Console.WriteLine($"{sp}X1: {p.X1}");
                Console.WriteLine($"{sp}Y1: {p.Y1}");
                Console.WriteLine($"{sp}X2: {p.X2}");
                Console.WriteLine($"{sp}Y2: {p.Y2}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRadialGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}X0: {p.X0}");
                Console.WriteLine($"{sp}Y0: {p.Y0}");
                Console.WriteLine($"{sp}Radius0: {p.Radius0}");
                Console.WriteLine($"{sp}X1: {p.X1}");
                Console.WriteLine($"{sp}Y1: {p.Y1}");
                Console.WriteLine($"{sp}Radius1: {p.Radius1}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                break;

            case PaintVarRadialGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}X0: {p.X0}");
                Console.WriteLine($"{sp}Y0: {p.Y0}");
                Console.WriteLine($"{sp}Radius0: {p.Radius0}");
                Console.WriteLine($"{sp}X1: {p.X1}");
                Console.WriteLine($"{sp}Y1: {p.Y1}");
                Console.WriteLine($"{sp}Radius1: {p.Radius1}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                break;

            case PaintSweepGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}StartAngle: {(float)p.StartAngle}");
                Console.WriteLine($"{sp}EndAngle: {(float)p.EndAngle}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                break;

            case PaintVarSweepGradient p:
                Console.WriteLine($"{sp}ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}StartAngle: {(float)p.StartAngle}");
                Console.WriteLine($"{sp}EndAngle: {(float)p.EndAngle}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                Console.WriteLine($"{sp}ColorLine: {p.ColorLine}");
                break;

            case PaintGlyph p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}GlyphID: {p.GlyphID}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintColrGlyph p:
                Console.WriteLine($"{sp}GlyphID: {p.GlyphID}");
                break;

            case PaintTransform p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}TransformOffset: {p.TransformOffset}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}Transform: {p.Transform}");
                break;

            case PaintVarTransform p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}TransformOffset: {p.TransformOffset}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}Transform: {p.Transform}");
                break;

            case PaintTranslate p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}DX: {p.DX}");
                Console.WriteLine($"{sp}DY: {p.DY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarTranslate p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}DX: {p.DX}");
                Console.WriteLine($"{sp}DY: {p.DY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScale p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}ScaleX: {(float)p.ScaleX}");
                Console.WriteLine($"{sp}ScaleY: {(float)p.ScaleY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarScale p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}ScaleX: {(float)p.ScaleX}");
                Console.WriteLine($"{sp}ScaleY: {(float)p.ScaleY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}ScaleX: {(float)p.ScaleX}");
                Console.WriteLine($"{sp}ScaleY: {(float)p.ScaleY}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarScaleAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}ScaleX: {(float)p.ScaleX}");
                Console.WriteLine($"{sp}ScaleY: {(float)p.ScaleY}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleUniform p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Scale: {(float)p.Scale}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarScaleUniform p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Scale: {(float)p.Scale}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleUniformAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Scale: {(float)p.Scale}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarScaleUniformAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Scale: {(float)p.Scale}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRotate p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Angle: {(float)p.Angle}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarRotate p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Angle: {(float)p.Angle}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRotateAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Angle: {(float)p.Angle}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarRotateAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}Angle: {(float)p.Angle}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintSkew p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}XSkewAngle: {(float)p.XSkewAngle}");
                Console.WriteLine($"{sp}YSkewAngle: {(float)p.YSkewAngle}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarSkew p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}XSkewAngle: {(float)p.XSkewAngle}");
                Console.WriteLine($"{sp}YSkewAngle: {(float)p.YSkewAngle}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintSkewAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}XSkewAngle: {(float)p.XSkewAngle}");
                Console.WriteLine($"{sp}YSkewAngle: {(float)p.YSkewAngle}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                break;

            case PaintVarSkewAroundCenter p:
                Console.WriteLine($"{sp}PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"{sp}XSkewAngle: {(float)p.XSkewAngle}");
                Console.WriteLine($"{sp}YSkewAngle: {(float)p.YSkewAngle}");
                Console.WriteLine($"{sp}CenterX: {p.CenterX}");
                Console.WriteLine($"{sp}CenterY: {p.CenterY}");
                Console.WriteLine($"{sp}Paint: {p.Paint}");
                Console.WriteLine($"{sp}VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintComposite p:
                Console.WriteLine($"{sp}SourcePaintOffset: {p.SourcePaintOffset}");
                Console.WriteLine($"{sp}CompositeMode: {p.CompositeMode}");
                Console.WriteLine($"{sp}BackdropPaintOffset: {p.BackdropPaintOffset}");
                Console.WriteLine($"{sp}SourcePaint: {p.SourcePaint}");
                Console.WriteLine($"{sp}BackdropPaint: {p.BackdropPaint}");
                break;
        }

        Console.WriteLine("");
    }
}
