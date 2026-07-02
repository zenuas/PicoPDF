using Mina.Command;
using Mina.Extension;
using OpenType;
using Pdf.Extension;
using Svg;
using Svg.Extension;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.TestAll;

public class SvgOutput : FontRegisterCommand
{
    [CommandOption("output"), CommandOption('o')]
    public TextWriter Output { get; init; } = Console.Out;

    [CommandOption("stroke")]
    public Color Stroke { get; init; } = Color.Black;

    [CommandOption("fill")]
    public Color Fill { get; init; } = Color.Transparent;

    [CommandOption("font")]
    public string Font { get; init; } = "Meiryo Bold";

    [CommandOption("point"), CommandOption('p')]
    public float Point { get; init; } = 100.0F;

    [CommandOption("joint-point"), CommandOption('j')]
    public float JointPoint { get; init; } = 2.0F;

    [CommandOption("debug")]
    public bool Debug { get; init; } = false;

    [CommandOption("point-format")]
    public string PointFormat { get; init; } = "F5";

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
        OutputSvg(font, [.. args.Select(arg => arg.ToUtf32CharArray().Select(x => (x, font.CharToGID(x))).ToArray())], "c");
        Output.Flush();
    }

    public void OutputSvg(IOpenTypeFont font, (int Char, uint GID)[][] cidss, string unique_id)
    {
        var top = 0f;
        var max_width = 0f;
        using var mem = new MemoryStream();
        using var writer = new StreamWriter(mem);
        foreach (var cids in cidss)
        {
            var (width, height) = OutputChar(font, writer, top, cids, unique_id);
            top += height;
            max_width = Math.Max(width, max_width);
        }
        writer.Flush();
        SvgExport.OutputBegin(Output, max_width, top, PointFormat);
        Output.Write(Encoding.UTF8.GetString(mem.ToArray()));
        SvgExport.OutputEnd(Output);
    }

    public (float Width, float Height) OutputChar(IOpenTypeFont font, TextWriter writer, float top, (int Char, uint GID)[] cids, string unique_id)
    {
        var outliness = cids.Select(x => font.GIDToOutline(x.GID, true)).ToArray();
        var total_width = cids.Select(x => font.GetAdvanceWidth(x.GID)).Sum();
        var ascent = font.HorizontalHeader.Ascender;
        var descent = font.HorizontalHeader.Descender;

        var surfaces = outliness.Flatten().GetSurfaces().ToArray();
        var gradient_layers = GetGradientLayers(surfaces);
        var (_, _, ymax, ymin) = surfaces.GetSurfaceSize();
        var scale = 1f / font.FontHeader.UnitsPerEm * Point;
        var left = 0f;
        var baseline = top + (ymax * scale);

        SvgExport.OutputDefs(writer, scale, left, baseline, gradient_layers, unique_id, Debug, PointFormat);
        for (var i = 0; i < cids.Length; i++)
        {
            writer.WriteLine();
            writer.WriteLine($"    <!-- {char.ConvertFromUtf32(cids[i].Char)} -->");
            SvgExport.OutputPath(writer, outliness[i], scale, left, baseline, gradient_layers, unique_id, Stroke, Fill, JointPoint, PointFormat);
            left += font.GetAdvanceWidth(cids[i].GID) * scale;
        }
        if (Debug)
        {
            writer.WriteLine($"    <!-- baseline -->");
            writer.WriteLine($"""    <line x1="0" y1="{baseline.ToPointString(PointFormat)}" x2="{(total_width * scale).ToPointString(PointFormat)}" y2="{baseline.ToPointString(PointFormat)}" stroke="red" />""");
        }
        return (total_width * scale, Math.Max(ascent.Value - descent.Value, ymax - ymin) * scale);
    }

    public static Dictionary<IColorLayer, int> GetGradientLayers(Surface[] surfaces)
    {
        var gradient_layers = new Dictionary<IColorLayer, int>();
        foreach (var color_layer in surfaces.Where(x => x.ColorLayer is { }).Select(x => x.ColorLayer!))
        {
            if (color_layer is not SolidColorLayer) _ = gradient_layers.TryAdd(color_layer, gradient_layers.Count);
        }
        return gradient_layers;
    }
}
