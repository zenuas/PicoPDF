using Svg.Outline;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Svg;

public static class Export
{
    public static void OutputBegin(TextWriter writer, float width, float height, string format) => writer.WriteLine($"""<svg width="{width.ToString(format)}" height="{height.ToString(format)}" xmlns="http://www.w3.org/2000/svg">""");

    public static void OutputEnd(TextWriter writer) => writer.WriteLine("</svg>");

    public static void OutputPath(
            TextWriter writer,
            IOutline[] outlines,
            float r,
            float left,
            float baseline,
            Dictionary<IColorLayer, int> gradient_layers,
            string unique_id,
            Color stroke,
            Color fill,
            float joint,
            string format
        )
    {
        var d = new StringBuilder();
        var c = new StringBuilder();
        OutputPath(writer, outlines, d, c, r, left, baseline, gradient_layers, unique_id, stroke, fill, joint, format);
        writer.Write(d);
        writer.Write(c);
    }

    public static void OutputPath(
            TextWriter writer,
            IOutline[] outlines,
            StringBuilder d,
            StringBuilder c,
            float r,
            float left,
            float baseline,
            Dictionary<IColorLayer, int> gradient_layers,
            string unique_id,
            Color stroke,
            Color fill,
            float joint,
            string format
        )
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
                                _ = d.AppendLine($"""    <path fill="url(#{unique_id}_{id})" fill-rule="evenodd" """);
                            }
                            else
                            {
                                var color = (surface.ColorLayer as SolidColorLayer)?.Color;
                                _ = d.AppendLine($"""    <path stroke="{ColorToHex(color ?? stroke)}" fill="{ColorToHex(color ?? fill)}" fill-rule="evenodd" """);
                            }
                            _ = d.Append("       d=\"");
                            isfirst = false;
                        }
                        var start = surface.Edges.First().Start;
                        if (joint > 0) _ = c.AppendLine($"""    <circle cx="{(left + (start.X * r)).ToString(format)}" cy="{(baseline - (start.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="blue" />""");
                        _ = d.AppendLine();
                        _ = d.AppendLine($"          M {(left + (start.X * r)).ToString(format)} {(baseline - (start.Y * r)).ToString(format)}");
                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    if (joint > 0) _ = c.AppendLine($"""    <circle cx="{(left + (line.End.X * r)).ToString(format)}" cy="{(baseline - (line.End.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="blue" />""");
                                    _ = d.AppendLine($"          L {(left + (line.End.X * r)).ToString(format)} {(baseline - (line.End.Y * r)).ToString(format)}");
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                                    {
                                        var cp = bezier.ControlPoint[0];
                                        if (joint > 0)
                                        {
                                            _ = c.AppendLine($"""    <circle cx="{(left + (cp.X * r)).ToString(format)}" cy="{(baseline - (cp.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{(left + (bezier.End.X * r)).ToString(format)}" cy="{(baseline - (bezier.End.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = d.AppendLine($"          Q {(left + (cp.X * r)).ToString(format)} {(baseline - (cp.Y * r)).ToString(format)}, {(left + (bezier.End.X * r)).ToString(format)} {(baseline - (bezier.End.Y * r)).ToString(format)}");
                                        break;
                                    }

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        if (joint > 0)
                                        {
                                            _ = c.AppendLine($"""    <circle cx="{(left + (cp1.X * r)).ToString(format)}" cy="{(baseline - (cp1.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{(left + (cp2.X * r)).ToString(format)}" cy="{(baseline - (cp2.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="red" />""");
                                            _ = c.AppendLine($"""    <circle cx="{(left + (bezier.End.X * r)).ToString(format)}" cy="{(baseline - (bezier.End.Y * r)).ToString(format)}" r="{joint.ToString(format)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = d.AppendLine($"          C {(left + (cp1.X * r)).ToString(format)} {(baseline - (cp1.Y * r)).ToString(format)}, {(left + (cp2.X * r)).ToString(format)} {(baseline - (cp2.Y * r)).ToString(format)}, {(left + (bezier.End.X * r)).ToString(format)} {(baseline - (bezier.End.Y * r)).ToString(format)}");
                                        break;
                                    }
                            }
                        }
                        _ = d.Append("          Z");
                        break;
                    }

                case Layer layer:
                    OutputPath(writer, layer.Surfaces, layer_d, c, r, left, baseline, gradient_layers, unique_id, stroke, fill, joint, format);
                    break;
            }
        }
        if (!isfirst) _ = d.AppendLine("\" />");
        _ = d.Append(layer_d);
    }

    public static void OutputDefs(
            TextWriter writer,
            float r,
            float left,
            float baseline,
            Dictionary<IColorLayer, int> gradient_layers,
            string unique_id,
            bool isdebug,
            string format
        )
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
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(r) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({m.M11.ToString(format)}, {m.M12.ToString(format)}, {m.M21.ToString(format)}, {m.M22.ToString(format)}, {m.M31.ToString(format)}, {m.M32.ToString(format)})" """);
                        writer.Write($"""x1="{linear.XY1.X.ToString(format)}" """);
                        writer.Write($"""y1="{linear.XY1.Y.ToString(format)}" """);
                        writer.Write($"""x2="{linear.XY2.X.ToString(format)}" """);
                        writer.Write($"""y2="{linear.XY2.Y.ToString(format)}">""");
                    }
                    else
                    {
                        writer.Write($"""x1="{linear.XY1.X.ToString(format)}" """);
                        writer.Write($"""y1="{linear.XY1.Y.ToString(format)}" """);
                        writer.Write($"""x2="{linear.XY2.X.ToString(format)}" """);
                        writer.Write($"""y2="{linear.XY2.Y.ToString(format)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !linear.GradientTransform.IsIdentity)
                    {
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(r) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        var xy1 = Vector2.Transform(linear.XY1, m);
                        var xy2 = Vector2.Transform(linear.XY2, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""x1="{xy1.X.ToString(format)}" """);
                        writer.Write($"""y1="{xy1.Y.ToString(format)}" """);
                        writer.Write($"""x2="{xy2.X.ToString(format)}" """);
                        writer.Write($"""y2="{xy2.Y.ToString(format)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in linear.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{offset.ToString(format)}%" stop-color="{ColorToHex(color)}" stop-opacity="{(color.A / 255F).ToString(format)}" />""");
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
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(r) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({m.M11.ToString(format)}, {m.M12.ToString(format)}, {m.M21.ToString(format)}, {m.M22.ToString(format)}, {m.M31.ToString(format)}, {m.M32.ToString(format)})" """);
                        writer.Write($"""cx="{radial.Cxy.X.ToString(format)}" """);
                        writer.Write($"""cy="{radial.Cxy.Y.ToString(format)}" """);
                        writer.Write($"""fx="{radial.Fxy.X.ToString(format)}" """);
                        writer.Write($"""fy="{radial.Fxy.Y.ToString(format)}" """);
                        writer.Write($"""fr="{radial.Fr.ToString(format)}" """);
                        writer.Write($"""r="{radial.R.ToString(format)}">""");
                    }
                    else
                    {
                        writer.Write($"""cx="{(left + (radial.Cxy.X * r)).ToString(format)}" """);
                        writer.Write($"""cy="{(baseline - (radial.Cxy.Y * r)).ToString(format)}" """);
                        writer.Write($"""fx="{(left + (radial.Fxy.X * r)).ToString(format)}" """);
                        writer.Write($"""fy="{(baseline - (radial.Fxy.Y * r)).ToString(format)}" """);
                        writer.Write($"""fr="{(radial.Fr * r).ToString(format)}" """);
                        writer.Write($"""r="{(radial.R * r).ToString(format)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !radial.GradientTransform.IsIdentity)
                    {
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(r) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        var cxy = Vector2.Transform(radial.Cxy, m);
                        var fxy = Vector2.Transform(radial.Fxy, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""cx="{cxy.X.ToString(format)}" """);
                        writer.Write($"""cy="{cxy.Y.ToString(format)}" """);
                        writer.Write($"""fx="{fxy.X.ToString(format)}" """);
                        writer.Write($"""fy="{fxy.Y.ToString(format)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in radial.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{offset}%" stop-color="{ColorToHex(color)}" stop-opacity="{(color.A / 255F).ToString(format)}" />""");
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
}
