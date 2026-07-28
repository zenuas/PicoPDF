using Svg.Outline;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Svg;

public static class SvgExport
{
    public static void OutputBegin(TextWriter writer, float width, float height, string format) => writer.WriteLine($"""<svg width="{width.ToString(format, CultureInfo.InvariantCulture)}" height="{height.ToString(format, CultureInfo.InvariantCulture)}" xmlns="http://www.w3.org/2000/svg">""");

    public static void OutputEnd(TextWriter writer) => writer.WriteLine("</svg>");

    public static void OutputPath(
            TextWriter writer,
            IOutline[] outlines,
            float scale,
            float left,
            float baseline,
            Dictionary<IColorLayer, int>? gradient_layers = null,
            string unique_id = "id",
            Color? stroke = null,
            Color? fill = null,
            float joint = 0,
            string format = "F5"
        )
    {
        var path = new StringBuilder();
        var control_point = new StringBuilder();
        OutputPath(writer, outlines, path, control_point, scale, left, baseline, gradient_layers ?? [], unique_id, stroke ?? Color.Black, fill ?? Color.Black, joint, format);
        writer.Write(path);
        writer.Write(control_point);
    }

    public static void OutputPath(
            TextWriter writer,
            IOutline[] outlines,
            StringBuilder path,
            StringBuilder control_point,
            float scale,
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
                                _ = path.AppendLine(CultureInfo.InvariantCulture, $"""    <path fill="url(#{unique_id}_{id})" fill-rule="evenodd" """);
                            }
                            else
                            {
                                var color = (surface.ColorLayer as SolidColorLayer)?.Color;
                                _ = path.AppendLine(CultureInfo.InvariantCulture, $"""    <path stroke="{ColorToHex(color ?? stroke)}" fill="{ColorToHex(color ?? fill)}" fill-rule="evenodd" """);
                            }
                            _ = path.Append("       d=\"");
                            isfirst = false;
                        }
                        var start = surface.Edges.First().Start;
                        if (joint > 0) _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (start.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (start.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="blue" />""");
                        _ = path.AppendLine();
                        _ = path.AppendLine(CultureInfo.InvariantCulture, $"          M {(left + (start.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (start.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}");
                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    if (joint > 0) _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (line.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (line.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="blue" />""");
                                    _ = path.AppendLine(CultureInfo.InvariantCulture, $"          L {(left + (line.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (line.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}");
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                                    {
                                        var cp = bezier.ControlPoint[0];
                                        if (joint > 0)
                                        {
                                            _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (cp.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (cp.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="red" />""");
                                            _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (bezier.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (bezier.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = path.AppendLine(CultureInfo.InvariantCulture, $"          Q {(left + (cp.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (cp.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}, {(left + (bezier.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (bezier.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}");
                                        break;
                                    }

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        if (joint > 0)
                                        {
                                            _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (cp1.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (cp1.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="red" />""");
                                            _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (cp2.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (cp2.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="red" />""");
                                            _ = control_point.AppendLine(CultureInfo.InvariantCulture, $"""    <circle cx="{(left + (bezier.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" cy="{(baseline - (bezier.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" r="{joint.ToString(format, CultureInfo.InvariantCulture)}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                        }
                                        _ = path.AppendLine(CultureInfo.InvariantCulture, $"          C {(left + (cp1.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (cp1.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}, {(left + (cp2.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (cp2.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}, {(left + (bezier.End.X * scale)).ToString(format, CultureInfo.InvariantCulture)} {(baseline - (bezier.End.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}");
                                        break;
                                    }
                            }
                        }
                        _ = path.Append("          Z");
                        break;
                    }

                case Layer layer:
                    OutputPath(writer, layer.Surfaces, layer_d, control_point, scale, left, baseline, gradient_layers, unique_id, stroke, fill, joint, format);
                    break;
            }
        }
        if (!isfirst) _ = path.AppendLine("\" />");
        _ = path.Append(layer_d);
    }

    public static void OutputDefs(
            TextWriter writer,
            float scale,
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
                    writer.Write($"""spreadMethod="{linear.SpreadMethod.ToString().ToLower(CultureInfo.InvariantCulture)}" """);
                    writer.Write($"""gradientUnits="userSpaceOnUse" """);
                    if (!linear.GradientTransform.IsIdentity)
                    {
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(scale) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({m.M11.ToString(format, CultureInfo.InvariantCulture)}, {m.M12.ToString(format, CultureInfo.InvariantCulture)}, {m.M21.ToString(format, CultureInfo.InvariantCulture)}, {m.M22.ToString(format, CultureInfo.InvariantCulture)}, {m.M31.ToString(format, CultureInfo.InvariantCulture)}, {m.M32.ToString(format, CultureInfo.InvariantCulture)})" """);
                        writer.Write($"""x1="{linear.XY1.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y1="{linear.XY1.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""x2="{linear.XY2.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y2="{linear.XY2.Y.ToString(format, CultureInfo.InvariantCulture)}">""");
                    }
                    else
                    {
                        writer.Write($"""x1="{linear.XY1.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y1="{linear.XY1.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""x2="{linear.XY2.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y2="{linear.XY2.Y.ToString(format, CultureInfo.InvariantCulture)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !linear.GradientTransform.IsIdentity)
                    {
                        var m = linear.GradientTransform * Matrix3x2.CreateScale(scale) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        var xy1 = Vector2.Transform(linear.XY1, m);
                        var xy2 = Vector2.Transform(linear.XY2, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""x1="{xy1.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y1="{xy1.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""x2="{xy2.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""y2="{xy2.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in linear.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{offset.ToString(format, CultureInfo.InvariantCulture)}%" stop-color="{ColorToHex(color)}" stop-opacity="{(color.A / 255F).ToString(format, CultureInfo.InvariantCulture)}" />""");
                    }
                    writer.WriteLine("        </linearGradient>");
                    break;

                case RadialGradientLayer radial:
                    writer.Write($"""        <radialGradient """);
                    writer.Write($"""id="{unique_id}_{id}" """);
                    writer.Write($"""spreadMethod="{radial.SpreadMethod.ToString().ToLower(CultureInfo.InvariantCulture)}" """);
                    writer.Write($"""gradientUnits="userSpaceOnUse" """);
                    if (!radial.GradientTransform.IsIdentity)
                    {
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(scale) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        writer.Write($"""gradientTransform="matrix({m.M11.ToString(format, CultureInfo.InvariantCulture)}, {m.M12.ToString(format, CultureInfo.InvariantCulture)}, {m.M21.ToString(format, CultureInfo.InvariantCulture)}, {m.M22.ToString(format, CultureInfo.InvariantCulture)}, {m.M31.ToString(format, CultureInfo.InvariantCulture)}, {m.M32.ToString(format, CultureInfo.InvariantCulture)})" """);
                        writer.Write($"""cx="{radial.Cxy.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""cy="{radial.Cxy.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fx="{radial.Fxy.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fy="{radial.Fxy.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fr="{radial.Fr.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""r="{radial.R.ToString(format, CultureInfo.InvariantCulture)}">""");
                    }
                    else
                    {
                        writer.Write($"""cx="{(left + (radial.Cxy.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""cy="{(baseline - (radial.Cxy.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fx="{(left + (radial.Fxy.X * scale)).ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fy="{(baseline - (radial.Fxy.Y * scale)).ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fr="{(radial.Fr * scale).ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""r="{(radial.R * scale).ToString(format, CultureInfo.InvariantCulture)}">""");
                    }
                    writer.WriteLine();
                    if (isdebug && !radial.GradientTransform.IsIdentity)
                    {
                        var m = radial.GradientTransform * Matrix3x2.CreateScale(scale) * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(left, baseline);
                        var cxy = Vector2.Transform(radial.Cxy, m);
                        var fxy = Vector2.Transform(radial.Fxy, m);
                        writer.Write($"""            <!-- """);
                        writer.Write($"""cx="{cxy.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""cy="{cxy.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fx="{fxy.X.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""fy="{fxy.Y.ToString(format, CultureInfo.InvariantCulture)}" """);
                        writer.Write($"""-->""");
                        writer.WriteLine();
                    }
                    foreach (var (offset, color) in radial.StopColors)
                    {
                        writer.WriteLine($"""            <stop offset="{offset}%" stop-color="{ColorToHex(color)}" stop-opacity="{(color.A / 255F).ToString(format, CultureInfo.InvariantCulture)}" />""");
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
