using Mina.Extension;
using Pdf.Documents;
using Pdf.Drawing;
using Pdf.Extension;
using Pdf.ExtGState;
using Pdf.Font;
using Pdf.Shading;
using Pdf.XObject.Form;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Pdf.Operation;

public class DrawPathOperations : IOperation, IHavePathOperations
{
    public required string Text { get; init; }
    public required IPathOperation[] Operations { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        if (option.Debug) writer.Write($"% {Text.ReplaceLineEndings("")}\n");
        writer.Write("q\n");
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Write("Q\n");
    }

    public static IEnumerable<DrawPathOperations> Create(string text, double basey, double left, double size, Type0Font font, Document document, IColor? color = null)
    {
        var opentype_font = font.EmbeddedFont ?? font.Font;
        var scale = 1f / opentype_font.FontHeader.UnitsPerEm * size;
        foreach (var c in text.ToUtf32CharArray())
        {
            var gid = opentype_font.CharToGID(c);
            var outlines = opentype_font.GIDToOutline(gid, true);

            foreach (var x in Create(outlines, c, scale, basey, left, size, font, document, color))
            {
                yield return x;
            }
            left += opentype_font.GetAdvanceWidth(gid) * scale;
        }
    }

    public static IEnumerable<DrawPathOperations> Create(IOutline[] outlines, int c, double scale, double basey, double left, double size, Type0Font font, Document document, IColor? color)
    {
        var opes = new List<IPathOperation>();
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface when surface.Edges.Length == 0:
                    // skip empty surface (SPACE, etc)
                    break;

                case Surface surface:
                    {
                        var start = surface.Edges.First().Start;
                        opes.Add(new DrawMovePath { Start = (new PointValue(left + (start.X * scale)), new PointValue(basey - (start.Y * scale))) });
                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    opes.Add(new DrawLinePath
                                    {
                                        End = (new PointValue(left + (line.End.X * scale)), new PointValue(basey - (line.End.Y * scale))),
                                    });
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                                    {
                                        var cp = bezier.ControlPoint[0];
                                        opes.Add(new DrawBezierCurvePath
                                        {
                                            ControlPoint1 = (new PointValue(left + ((cp.X + cp.X + bezier.Start.X) / 3.0f * scale)), new PointValue(basey - ((cp.Y + cp.Y + bezier.Start.Y) / 3.0f * scale))),
                                            ControlPoint2 = (new PointValue(left + ((cp.X + cp.X + bezier.End.X) / 3.0f * scale)), new PointValue(basey - ((cp.Y + cp.Y + bezier.End.Y) / 3.0f * scale))),
                                            End = (new PointValue(left + (bezier.End.X * scale)), new PointValue(basey - (bezier.End.Y * scale))),
                                        });
                                        break;
                                    }

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        opes.Add(new DrawBezierCurvePath
                                        {
                                            ControlPoint1 = (new PointValue(left + (cp1.X * scale)), new PointValue(basey - (cp1.Y * scale))),
                                            ControlPoint2 = (new PointValue(left + (cp2.X * scale)), new PointValue(basey - (cp2.Y * scale))),
                                            End = (new PointValue(left + (bezier.End.X * scale)), new PointValue(basey - (bezier.End.Y * scale))),
                                        });
                                        break;
                                    }
                            }
                        }
                        break;
                    }

                case Layer layer:
                    foreach (var x in Create(layer.Surfaces, c, scale, basey, left, size, font, document, color))
                    {
                        yield return x;
                    }
                    break;

                default:
                    throw new();
            }
        }
        if (opes.Count > 0)
        {
            switch (outlines.OfType<Surface>().Where(x => x.ColorLayer is { }).FirstOrDefault()?.ColorLayer)
            {
                case LinearGradientLayer linear when linear.StopColors.Length > 0:
                    {
                        opes.Add(new DrawAnyOperator { Operator = "h" });
                        opes.Add(new DrawAnyOperator { Operator = "W*" });
                        opes.Add(new DrawAnyOperator { Operator = "n" });
                        opes.Add(new DrawTransformationMatrix
                        {
                            Transform =
                                linear.GradientTransform *
                                Matrix3x2.CreateScale((float)scale) *
                                Matrix3x2.CreateScale(1, -1) *
                                Matrix3x2.CreateTranslation((float)left, (float)basey),
                        });
                        if (linear.StopColors.Any(x => x.StopColor.A < 255))
                        {
                            var shading = document.AddShading(AxialShading.Create($"sh{document.PdfObjects.Count}", linear, ColorToAlpha, "/DeviceGray"));
                            // Set a sufficiently large enough for the BBox.
                            var minx = Math.Min(linear.XY1.X, linear.XY2.X);
                            var miny = Math.Min(linear.XY1.Y, linear.XY2.Y);
                            var maxx = Math.Max(linear.XY1.X, linear.XY2.X);
                            var maxy = Math.Max(linear.XY1.Y, linear.XY2.Y);
                            var width = Math.Max(Math.Abs(maxx), Math.Abs(minx)) * 2;
                            var height = Math.Max(Math.Abs(maxy), Math.Abs(miny)) * 2;
                            var g = FormXObject.CreateTransparency(minx - width, miny - height, maxx + width, maxy + height, shading);
                            var gstream = g.GetWriteStream(false);
                            new DrawPathShading { Shading = shading }.OperationWrite(0, 0, gstream, new());
                            gstream.Flush();
                            opes.Add(new DrawPathExtGState { ExtGState = document.AddGraphicsStateParameter(GraphicsStateParameter.CreateTransparency($"gs{document.PdfObjects.Count}", g)) });
                        }
                        opes.Add(new DrawPathShading { Shading = document.AddShading(AxialShading.Create($"sh{document.PdfObjects.Count}", linear, ColorToRGB, "/DeviceRGB")) });
                        break;
                    }

                case RadialGradientLayer radial when radial.StopColors.Length > 0:
                    {
                        opes.Add(new DrawAnyOperator { Operator = "h" });
                        opes.Add(new DrawAnyOperator { Operator = "W*" });
                        opes.Add(new DrawAnyOperator { Operator = "n" });
                        opes.Add(new DrawTransformationMatrix
                        {
                            Transform =
                                radial.GradientTransform *
                                Matrix3x2.CreateScale((float)scale) *
                                Matrix3x2.CreateScale(1, -1) *
                                Matrix3x2.CreateTranslation((float)left, (float)basey),
                        });
                        if (radial.StopColors.Any(x => x.StopColor.A < 255))
                        {
                            var shading = document.AddShading(RadialShading.Create($"sh{document.PdfObjects.Count}", radial, ColorToAlpha, "/DeviceGray"));
                            // Set a sufficiently large enough for the BBox.
                            var minx = Math.Min(radial.Fxy.X, radial.Cxy.X);
                            var miny = Math.Min(radial.Fxy.Y, radial.Cxy.Y);
                            var maxx = Math.Max(radial.Fxy.X, radial.Cxy.X);
                            var maxy = Math.Max(radial.Fxy.Y, radial.Cxy.Y);
                            var maxr = Math.Max(radial.Fr, radial.R);
                            var g = FormXObject.CreateTransparency(minx - maxr, miny - maxr, maxx + maxr, maxy + maxr, shading);
                            var gstream = g.GetWriteStream(false);
                            new DrawPathShading { Shading = shading }.OperationWrite(0, 0, gstream, new());
                            gstream.Flush();
                            opes.Add(new DrawPathExtGState { ExtGState = document.AddGraphicsStateParameter(GraphicsStateParameter.CreateTransparency($"gs{document.PdfObjects.Count}", g)) });
                        }
                        opes.Add(new DrawPathShading { Shading = document.AddShading(RadialShading.Create($"sh{document.PdfObjects.Count}", radial, ColorToRGB, "/DeviceRGB")) });
                        break;
                    }

                case SolidColorLayer solid_color:
                    opes.Add(new DrawAnyOperator { Operator = solid_color.Color.ToDeviceRGB().CreateColor(false) });
                    opes.Add(new DrawAnyOperator { Operator = "f*" });
                    break;

                case LinearGradientLayer:
                case RadialGradientLayer:
                case null:
                    if (color is { }) opes.Add(new DrawAnyOperator { Operator = color.CreateColor(false) });
                    opes.Add(new DrawAnyOperator { Operator = "f*" });
                    break;

                default:
                    throw new();
            }
            yield return new() { Text = char.ConvertFromUtf32(c), Operations = [.. opes] };
        }
    }

    public static float[] ColorToRGB(Color color) => [color.R / 255f, color.G / 255f, color.B / 255f];

    public static float[] ColorToAlpha(Color color) => [color.A / 255f];
}
