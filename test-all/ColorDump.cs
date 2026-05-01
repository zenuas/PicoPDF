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

            Console.WriteLine($"Char: {char.ConvertFromUtf32(cid)}  (U+{(cid <= 0xFFFF ? cid.ToString("X4") : cid.ToString("X"))}");
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
                EnumPaint(font.Color.BaseGlyphListRecord.Paints[i], font.Color);
            }
        }
    }

    public static void EnumPaint(IPaintFormat paint, ColorTable colr)
    {
        DumpPaint(paint, colr);

        if (paint is PaintColrLayers paintColrLayers)
        {
            foreach (var layer in colr.LayerListRecord!.Paints[(int)paintColrLayers.FirstLayerIndex..((int)paintColrLayers.FirstLayerIndex + paintColrLayers.NumberOfLayers)])
            {
                EnumPaint(layer, colr);
            }
        }
        else if (paint is PaintComposite paintComposite)
        {
            EnumPaint(paintComposite.SourcePaint, colr);
            EnumPaint(paintComposite.BackdropPaint, colr);
        }
        else
        {
            if (paint is IHavePaint subpaint)
            {
                EnumPaint(subpaint.Paint, colr);
            }
        }
    }

    public static void DumpPaint(IPaintFormat paint, ColorTable colr)
    {
        Console.WriteLine($"Format: {paint.Format} ({paint.GetType().Name})");

        switch (paint)
        {
            case PaintColrLayers p:
                Console.WriteLine($"  NumberOfLayers: {p.NumberOfLayers}");
                Console.WriteLine($"  FirstLayerIndex: {p.FirstLayerIndex}");
                break;

            case PaintSolid p:
                Console.WriteLine($"  PaletteIndex: {p.PaletteIndex}");
                Console.WriteLine($"  Alpha: {p.Alpha}");
                break;

            case PaintVarSolid p:
                Console.WriteLine($"  PaletteIndex: {p.PaletteIndex}");
                Console.WriteLine($"  Alpha: {p.Alpha}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintLinearGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  X0: {p.X0}");
                Console.WriteLine($"  Y0: {p.Y0}");
                Console.WriteLine($"  X1: {p.X1}");
                Console.WriteLine($"  Y1: {p.Y1}");
                Console.WriteLine($"  X2: {p.X2}");
                Console.WriteLine($"  Y2: {p.Y2}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                break;

            case PaintVarLinearGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  X0: {p.X0}");
                Console.WriteLine($"  Y0: {p.Y0}");
                Console.WriteLine($"  X1: {p.X1}");
                Console.WriteLine($"  Y1: {p.Y1}");
                Console.WriteLine($"  X2: {p.X2}");
                Console.WriteLine($"  Y2: {p.Y2}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRadialGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  X0: {p.X0}");
                Console.WriteLine($"  Y0: {p.Y0}");
                Console.WriteLine($"  Radius0: {p.Radius0}");
                Console.WriteLine($"  X1: {p.X1}");
                Console.WriteLine($"  Y1: {p.Y1}");
                Console.WriteLine($"  Radius1: {p.Radius1}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                break;

            case PaintVarRadialGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  X0: {p.X0}");
                Console.WriteLine($"  Y0: {p.Y0}");
                Console.WriteLine($"  Radius0: {p.Radius0}");
                Console.WriteLine($"  X1: {p.X1}");
                Console.WriteLine($"  Y1: {p.Y1}");
                Console.WriteLine($"  Radius1: {p.Radius1}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                break;

            case PaintSweepGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  StartAngle: {p.StartAngle}");
                Console.WriteLine($"  EndAngle: {p.EndAngle}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                break;

            case PaintVarSweepGradient p:
                Console.WriteLine($"  ColorLineOffset: {p.ColorLineOffset}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  StartAngle: {p.StartAngle}");
                Console.WriteLine($"  EndAngle: {p.EndAngle}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                Console.WriteLine($"  ColorLine: {p.ColorLine}");
                break;

            case PaintGlyph p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  GlyphID: {p.GlyphID}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintColrGlyph p:
                Console.WriteLine($"  GlyphID: {p.GlyphID}");
                break;

            case PaintTransform p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  TransformOffset: {p.TransformOffset}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  Transform: {p.Transform}");
                break;

            case PaintVarTransform p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  TransformOffset: {p.TransformOffset}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  Transform: {p.Transform}");
                break;

            case PaintTranslate p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  DX: {p.DX}");
                Console.WriteLine($"  DY: {p.DY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarTranslate p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  DX: {p.DX}");
                Console.WriteLine($"  DY: {p.DY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScale p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  ScaleX: {p.ScaleX}");
                Console.WriteLine($"  ScaleY: {p.ScaleY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarScale p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  ScaleX: {p.ScaleX}");
                Console.WriteLine($"  ScaleY: {p.ScaleY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  ScaleX: {p.ScaleX}");
                Console.WriteLine($"  ScaleY: {p.ScaleY}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarScaleAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  ScaleX: {p.ScaleX}");
                Console.WriteLine($"  ScaleY: {p.ScaleY}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleUniform p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Scale: {p.Scale}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarScaleUniform p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Scale: {p.Scale}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintScaleUniformAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Scale: {p.Scale}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarScaleUniformAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Scale: {p.Scale}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRotate p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Angle: {p.Angle}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarRotate p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Angle: {p.Angle}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintRotateAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Angle: {p.Angle}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarRotateAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  Angle: {p.Angle}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintSkew p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  XSkewAngle: {p.XSkewAngle}");
                Console.WriteLine($"  YSkewAngle: {p.YSkewAngle}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarSkew p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  XSkewAngle: {p.XSkewAngle}");
                Console.WriteLine($"  YSkewAngle: {p.YSkewAngle}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintSkewAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  XSkewAngle: {p.XSkewAngle}");
                Console.WriteLine($"  YSkewAngle: {p.YSkewAngle}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                break;

            case PaintVarSkewAroundCenter p:
                Console.WriteLine($"  PaintOffset: {p.PaintOffset}");
                Console.WriteLine($"  XSkewAngle: {p.XSkewAngle}");
                Console.WriteLine($"  YSkewAngle: {p.YSkewAngle}");
                Console.WriteLine($"  CenterX: {p.CenterX}");
                Console.WriteLine($"  CenterY: {p.CenterY}");
                Console.WriteLine($"  Paint: {p.Paint}");
                Console.WriteLine($"  VarIndexBase: {p.VarIndexBase}");
                break;

            case PaintComposite p:
                Console.WriteLine($"  SourcePaintOffset: {p.SourcePaintOffset}");
                Console.WriteLine($"  CompositeMode: {p.CompositeMode}");
                Console.WriteLine($"  BackdropPaintOffset: {p.BackdropPaintOffset}");
                Console.WriteLine($"  SourcePaint: {p.SourcePaint}");
                Console.WriteLine($"  BackdropPaint: {p.BackdropPaint}");
                break;
        }

        Console.WriteLine("");
    }
}
