using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Outline;
using PicoPDF.Pdf.Operation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
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

    public static readonly Matrix3x2 FlipY = Matrix3x2.CreateScale(1, -1);

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
            var (width, height) = OutputPath(font, writer, top, cids, unique_id);
            top += height;
            max_width = Math.Max(width, max_width);
        }
        writer.Flush();
        Output.WriteLine($"""<svg width="{IOperation.PointToString(max_width, PointFormat)}" height="{IOperation.PointToString(top, PointFormat)}" xmlns="http://www.w3.org/2000/svg">""");
        Output.Write(Encoding.UTF8.GetString(mem.ToArray()));
        Output.WriteLine("</svg>");
    }

    public (float Width, float Height) OutputPath(IOpenTypeFont font, TextWriter writer, float top, (int Char, uint GID)[] cids, string unique_id)
    {
        var outliness = cids.Select(x => font.GIDToOutline(x.GID, true)).ToArray();
        var total_width = cids.Select(x => font.GetAdvanceWidth(x.GID)).Sum();
        var ascent = font.HorizontalHeader.Ascender;
        var descent = font.HorizontalHeader.Descender;

        var surfaces = Utility.GetSurfaces(outliness.Flatten()).ToArray();
        var gradient_layers = GetGradientLayers(surfaces);
        var (_, _, ymax, ymin) = Utility.GetSurfaceSize(surfaces);
        var r = 1f / font.FontHeader.UnitsPerEm * Point;
        var left = 0f;
        var baseline = top + (ymax * r);

        OutputDefs(writer, r, left, baseline, gradient_layers, unique_id, Debug);
        for (var i = 0; i < cids.Length; i++)
        {
            writer.WriteLine();
            writer.WriteLine($"    <!-- {char.ConvertFromUtf32(cids[i].Char)} -->");
            var d = new StringBuilder();
            var c = new StringBuilder();
            OutputPath(outliness[i], d, c, r, left, baseline, gradient_layers, unique_id);
            writer.Write(d);
            writer.Write(c);
            left += font.GetAdvanceWidth(cids[i].GID) * r;
        }
        if (Debug)
        {
            writer.WriteLine($"    <!-- baseline -->");
            writer.WriteLine($"""    <line x1="0" y1="{IOperation.PointToString(baseline, PointFormat)}" x2="{IOperation.PointToString(total_width * r, PointFormat)}" y2="{IOperation.PointToString(baseline, PointFormat)}" stroke="red" />""");
        }
        return (total_width * r, Math.Max(ascent - descent, ymax - ymin) * r);
    }

    public void OutputPath(IOutline[] outlines, StringBuilder d, StringBuilder c, float r, float left, float baseline, Dictionary<IColorLayer, int> gradient_layers, string unique_id)
    {
        var layer_d = new StringBuilder();
        var colo = Utility.GetSurfaces(outlines).Where(x => x.ColorLayer is { }).Select(x => x.ColorLayer!);

        var isfirst = true;
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface when surface.Edges.Length == 0:
                    // skip empty surface (SPACE, etc)
                    break;

                case Surface surface:
                    {
                        if (isfirst)
                        {
                            if (surface.ColorLayer is { } && gradient_layers.TryGetValue(surface.ColorLayer, out var id))
                            {
                                _ = d.AppendLine($"""    <path fill="url(#{unique_id}_{IOperation.PointToString(id, PointFormat)})" fill-rule="evenodd" """);
                            }
                            else
                            {
                                var color = (surface.ColorLayer as SolidColorLayer)?.Color;
                                _ = d.AppendLine($"""    <path stroke="{ColorToHex(color ?? Stroke)}" fill="{ColorToHex(color ?? Fill)}" fill-rule="evenodd" """);
                            }
                            _ = d.Append("       d=\"");
                            isfirst = false;
                        }
                        var start = surface.Edges.First().Start;
                        if (JointPoint > 0) _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (start.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (start.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="blue" />""");
                        _ = d.AppendLine();
                        _ = d.AppendLine($"          M {IOperation.PointToString(left + (start.X * r), PointFormat)} {IOperation.PointToString(baseline - (start.Y * r), PointFormat)}");
                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    if (JointPoint > 0) _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (line.End.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (line.End.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="blue" />""");
                                    _ = d.AppendLine($"          L {IOperation.PointToString(left + (line.End.X * r), PointFormat)} {IOperation.PointToString(baseline - (line.End.Y * r), PointFormat)}");
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                                    {
                                        var cp = bezier.ControlPoint[0];
                                        if (JointPoint > 0)
                                        {
                                            _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (cp.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (cp.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (bezier.End.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (bezier.End.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = d.AppendLine($"          Q {IOperation.PointToString(left + (cp.X * r), PointFormat)} {IOperation.PointToString(baseline - (cp.Y * r), PointFormat)}, {IOperation.PointToString(left + (bezier.End.X * r), PointFormat)} {IOperation.PointToString(baseline - (bezier.End.Y * r), PointFormat)}");
                                        break;
                                    }

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        if (JointPoint > 0)
                                        {
                                            _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (cp1.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (cp1.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (cp2.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (cp2.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{IOperation.PointToString(left + (bezier.End.X * r), PointFormat)}" cy="{IOperation.PointToString(baseline - (bezier.End.Y * r), PointFormat)}" r="{IOperation.PointToString(JointPoint, PointFormat)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = d.AppendLine($"          C {IOperation.PointToString(left + (cp1.X * r), PointFormat)} {IOperation.PointToString(baseline - (cp1.Y * r), PointFormat)}, {IOperation.PointToString(left + (cp2.X * r), PointFormat)} {IOperation.PointToString(baseline - (cp2.Y * r), PointFormat)}, {IOperation.PointToString(left + (bezier.End.X * r), PointFormat)} {IOperation.PointToString(baseline - (bezier.End.Y * r), PointFormat)}");
                                        break;
                                    }
                            }
                        }
                        _ = d.Append("          Z");
                        break;
                    }

                case Layer layer:
                    OutputPath(layer.Surfaces, layer_d, c, r, left, baseline, gradient_layers, unique_id);
                    break;

                default:
                    throw new();
            }
        }
        if (!isfirst) _ = d.AppendLine("\" />");
        _ = d.Append(layer_d);
    }

    public void OutputDefs(TextWriter writer, float r, float left, float baseline, Dictionary<IColorLayer, int> gradient_layers, string unique_id, bool isdebug)
    {
        writer.WriteLine("    <defs>");
        foreach (var (color_layer, id) in gradient_layers)
        {
            switch (color_layer)
            {
                case LinearGradientLayer linear:
                    writer.Write($"""        <linearGradient """);
                    writer.Write($"""id="{unique_id}_{id}" """);
                    writer.Write($"""spreadMethod="{linear.SpreadMethod.ToString().ToLower()}" """);
                    writer.Write($"""gradientUnits="userSpaceOnUse" """);
                    if (!linear.GradientTransform.IsIdentity)
                    {
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(r) * FlipY * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({IOperation.PointToString(m.M11, PointFormat)}, {IOperation.PointToString(m.M12, PointFormat)}, {IOperation.PointToString(m.M21, PointFormat)}, {IOperation.PointToString(m.M22, PointFormat)}, {IOperation.PointToString(m.M31, PointFormat)}, {IOperation.PointToString(m.M32, PointFormat)})" """);
                        writer.Write($"""x1="{IOperation.PointToString(linear.XY1.X, PointFormat)}" """);
                        writer.Write($"""y1="{IOperation.PointToString(linear.XY1.Y, PointFormat)}" """);
                        writer.Write($"""x2="{IOperation.PointToString(linear.XY2.X, PointFormat)}" """);
                        writer.Write($"""y2="{IOperation.PointToString(linear.XY2.Y, PointFormat)}">""");
                    }
                    else
                    {
                        writer.Write($"""x1="{IOperation.PointToString(left + (linear.XY1.X * r), PointFormat)}" """);
                        writer.Write($"""y1="{IOperation.PointToString(baseline - (linear.XY1.Y * r), PointFormat)}" """);
                        writer.Write($"""x2="{IOperation.PointToString(left + (linear.XY2.X * r), PointFormat)}" """);
                        writer.Write($"""y2="{IOperation.PointToString(baseline - (linear.XY2.Y * r), PointFormat)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !linear.GradientTransform.IsIdentity)
                    {
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(r) * FlipY * Matrix3x2.CreateTranslation(left, baseline);
                        var xy1 = Vector2.Transform(linear.XY1, m);
                        var xy2 = Vector2.Transform(linear.XY2, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""x1="{IOperation.PointToString(xy1.X, PointFormat)}" """);
                        writer.Write($"""y1="{IOperation.PointToString(xy1.Y, PointFormat)}" """);
                        writer.Write($"""x2="{IOperation.PointToString(xy2.X, PointFormat)}" """);
                        writer.Write($"""y2="{IOperation.PointToString(xy2.Y, PointFormat)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in linear.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{IOperation.PointToString(offset, PointFormat)}%" stop-color="{ColorToHex(color)}" stop-opacity="{IOperation.PointToString(color.A / 255F, PointFormat)}" />""");
                    }
                    writer.WriteLine("        </linearGradient>");
                    break;

                case RadialGradientLayer radial:
                    writer.Write($"""        <radialGradient """);
                    writer.Write($"""id="{unique_id}_{id}" """);
                    writer.Write($"""spreadMethod="{radial.SpreadMethod.ToString().ToLower()}" """);
                    writer.Write($"""gradientUnits="userSpaceOnUse" """);
                    if (!radial.GradientTransform.IsIdentity)
                    {
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(r) * FlipY * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({IOperation.PointToString(m.M11, PointFormat)}, {IOperation.PointToString(m.M12, PointFormat)}, {IOperation.PointToString(m.M21, PointFormat)}, {IOperation.PointToString(m.M22, PointFormat)}, {IOperation.PointToString(m.M31, PointFormat)}, {IOperation.PointToString(m.M32, PointFormat)})" """);
                        writer.Write($"""cx="{IOperation.PointToString(radial.Cxy.X, PointFormat)}" """);
                        writer.Write($"""cy="{IOperation.PointToString(radial.Cxy.Y, PointFormat)}" """);
                        writer.Write($"""fx="{IOperation.PointToString(radial.Fxy.X, PointFormat)}" """);
                        writer.Write($"""fy="{IOperation.PointToString(radial.Fxy.Y, PointFormat)}" """);
                        writer.Write($"""fr="{IOperation.PointToString(radial.Fr, PointFormat)}" """);
                        writer.Write($"""r="{IOperation.PointToString(radial.R, PointFormat)}">""");
                    }
                    else
                    {
                        writer.Write($"""cx="{IOperation.PointToString(left + (radial.Cxy.X * r), PointFormat)}" """);
                        writer.Write($"""cy="{IOperation.PointToString(baseline - (radial.Cxy.Y * r), PointFormat)}" """);
                        writer.Write($"""fx="{IOperation.PointToString(left + (radial.Fxy.X * r), PointFormat)}" """);
                        writer.Write($"""fy="{IOperation.PointToString(baseline - (radial.Fxy.Y * r), PointFormat)}" """);
                        writer.Write($"""fr="{IOperation.PointToString(radial.Fr * r, PointFormat)}" """);
                        writer.Write($"""r="{IOperation.PointToString(radial.R * r, PointFormat)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !radial.GradientTransform.IsIdentity)
                    {
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(r) * FlipY * Matrix3x2.CreateTranslation(left, baseline);
                        var cxy = Vector2.Transform(radial.Cxy, m);
                        var fxy = Vector2.Transform(radial.Fxy, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""cx="{IOperation.PointToString(cxy.X, PointFormat)}" """);
                        writer.Write($"""cy="{IOperation.PointToString(cxy.Y, PointFormat)}" """);
                        writer.Write($"""fx="{IOperation.PointToString(fxy.X, PointFormat)}" """);
                        writer.Write($"""fy="{IOperation.PointToString(fxy.Y, PointFormat)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in radial.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{offset}%" stop-color="{ColorToHex(color)}" stop-opacity="{IOperation.PointToString(color.A / 255F, PointFormat)}" />""");
                    }
                    writer.WriteLine("        </radialGradient>");
                    break;

                default:
                    throw new();
            }
        }
        writer.WriteLine("    </defs>");
    }

    public static string ColorToHex(Color color) => color == Color.Transparent ? "transparent" : $"#{color.R:X2}{color.G:X2}{color.B:X2}";

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
