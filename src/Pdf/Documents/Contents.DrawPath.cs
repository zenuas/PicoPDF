using Mina.Extension;
using OpenType;
using OpenType.Outline;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.ExtGState;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.Function;
using PicoPDF.Pdf.Operation;
using PicoPDF.Pdf.Shading;
using PicoPDF.Pdf.SoftMasks;
using PicoPDF.Pdf.XObject.Form;
using PicoPDF.Pdf.XObject.Group;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public static IEnumerable<DrawPathOperations> CreateDrawPathOnBaselineOperation(string text, double basey, double left, double size, Type0Font font, Document document, IColor? color = null)
    {
        var opentype_font = font.EmbeddedFont ?? font.Font;
        var r = (1f / opentype_font.FontHeader.UnitsPerEm) * size;
        foreach (var c in text.ToUtf32CharArray())
        {
            var gid = opentype_font.CharToGID(c);
            var outlines = opentype_font.GIDToOutline(gid, true);

            foreach (var x in CreateDrawPathOnBaselineOperation(outlines, c, r, basey, left, size, font, document, color))
            {
                yield return x;
            }
            left += opentype_font.GetAdvanceWidth(gid) * r;
        }
    }

    public static IEnumerable<DrawPathOperations> CreateDrawPathOnBaselineOperation(IOutline[] outlines, int c, double r, double basey, double left, double size, Type0Font font, Document document, IColor? color)
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
                        opes.Add(new DrawMovePath { Start = (new PointValue(left + (start.X * r)), new PointValue(basey - (start.Y * r))) });
                        foreach (var edge in surface.Edges)
                        {
                            switch (edge)
                            {
                                case Line line:
                                    opes.Add(new DrawLinePath
                                    {
                                        End = (new PointValue(left + (line.End.X * r)), new PointValue(basey - (line.End.Y * r))),
                                    });
                                    break;

                                case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                                    {
                                        var cp = bezier.ControlPoint[0];
                                        opes.Add(new DrawBezierCurvePath
                                        {
                                            ControlPoint1 = (new PointValue(left + (((cp.X + cp.X + bezier.Start.X) / 3.0f) * r)), new PointValue(basey - (((cp.Y + cp.Y + bezier.Start.Y) / 3.0f) * r))),
                                            ControlPoint2 = (new PointValue(left + (((cp.X + cp.X + bezier.End.X) / 3.0f) * r)), new PointValue(basey - (((cp.Y + cp.Y + bezier.End.Y) / 3.0f) * r))),
                                            End = (new PointValue(left + (bezier.End.X * r)), new PointValue(basey - (bezier.End.Y * r))),
                                        });
                                        break;
                                    }

                                case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                                    {
                                        var cp1 = bezier.ControlPoint[0];
                                        var cp2 = bezier.ControlPoint[1];
                                        opes.Add(new DrawBezierCurvePath
                                        {
                                            ControlPoint1 = (new PointValue(left + (cp1.X * r)), new PointValue(basey - (cp1.Y * r))),
                                            ControlPoint2 = (new PointValue(left + (cp2.X * r)), new PointValue(basey - (cp2.Y * r))),
                                            End = (new PointValue(left + (bezier.End.X * r)), new PointValue(basey - (bezier.End.Y * r))),
                                        });
                                        break;
                                    }
                            }
                        }
                        break;
                    }

                case Layer layer:
                    foreach (var x in CreateDrawPathOnBaselineOperation(layer.Surfaces, c, r, basey, left, size, font, document, color))
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
                                Matrix3x2.CreateScale((float)r) *
                                Matrix3x2.CreateScale(1, -1) *
                                Matrix3x2.CreateTranslation((float)left, (float)basey),
                        });
                        if (linear.StopColors.Any(x => x.StopColor.A < 255))
                        {
                            var shading = CreateAxialShading(document, linear, ColorToAlpha, "/DeviceGray");
                            // Set a sufficiently large enough for the BBox.
                            var minx = Math.Min(linear.XY1.X, linear.XY2.X);
                            var miny = Math.Min(linear.XY1.Y, linear.XY2.Y);
                            var maxx = Math.Max(linear.XY1.X, linear.XY2.X);
                            var maxy = Math.Max(linear.XY1.Y, linear.XY2.Y);
                            var width = Math.Max(Math.Abs(maxx), Math.Abs(minx)) * 2;
                            var height = Math.Max(Math.Abs(maxy), Math.Abs(miny)) * 2;
                            var g = CreateTransparencyFormXObject(minx - width, miny - height, maxx + width, maxy + height, shading);
                            var gstream = g.GetWriteStream(false);
                            (new DrawPathShading { Shading = shading }).OperationWrite(0, 0, gstream, new());
                            gstream.Flush();
                            opes.Add(new DrawPathExtGState { ExtGState = CreateTransparencyGraphicsStateParameter(document, g) });
                        }
                        opes.Add(new DrawPathShading { Shading = CreateAxialShading(document, linear, ColorToRGB, "/DeviceRGB") });
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
                                Matrix3x2.CreateScale((float)r) *
                                Matrix3x2.CreateScale(1, -1) *
                                Matrix3x2.CreateTranslation((float)left, (float)basey),
                        });
                        if (radial.StopColors.Any(x => x.StopColor.A < 255))
                        {
                            var shading = CreateRadialShading(document, radial, ColorToAlpha, "/DeviceGray");
                            // Set a sufficiently large enough for the BBox.
                            var minx = Math.Min(radial.Fxy.X, radial.Cxy.X);
                            var miny = Math.Min(radial.Fxy.Y, radial.Cxy.Y);
                            var maxx = Math.Max(radial.Fxy.X, radial.Cxy.X);
                            var maxy = Math.Max(radial.Fxy.Y, radial.Cxy.Y);
                            var maxr = Math.Max(radial.Fr, radial.R);
                            var g = CreateTransparencyFormXObject(minx - maxr, miny - maxr, maxx + maxr, maxy + maxr, shading);
                            var gstream = g.GetWriteStream(false);
                            (new DrawPathShading { Shading = shading }).OperationWrite(0, 0, gstream, new());
                            gstream.Flush();
                            opes.Add(new DrawPathExtGState { ExtGState = CreateTransparencyGraphicsStateParameter(document, g) });
                        }
                        opes.Add(new DrawPathShading { Shading = CreateRadialShading(document, radial, ColorToRGB, "/DeviceRGB") });
                        break;
                    }

                case SolidColorLayer solid_color:
                    opes.Add(new DrawAnyOperator { Operator = PdfUtility.ToDeviceRGB(solid_color.Color).CreateColor(false) });
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

    public static AxialShading CreateAxialShading(Document document, LinearGradientLayer linear, Func<Color, float[]> f, string color_space)
    {
        var shading = new AxialShading
        {
            Name = $"sh{document.PdfObjects.Count}",
            ColorSpace = color_space,
            Coords = (
                    new PointValue(linear.XY1.X),
                    new PointValue(linear.XY1.Y),
                    new PointValue(linear.XY2.X),
                    new PointValue(linear.XY2.Y)
                ),
            Function = StopColorsToFunction(linear.StopColors, f),
            Extend = (true, true),
        };
        _ = document.AddShading(shading);
        return shading;
    }

    public static RadialShading CreateRadialShading(Document document, RadialGradientLayer radial, Func<Color, float[]> f, string color_space)
    {
        var shading = new RadialShading
        {
            Name = $"sh{document.PdfObjects.Count}",
            ColorSpace = color_space,
            Coords = (
                    new PointValue(radial.Fxy.X),
                    new PointValue(radial.Fxy.Y),
                    new PointValue(radial.Fr),
                    new PointValue(radial.Cxy.X),
                    new PointValue(radial.Cxy.Y),
                    new PointValue(radial.R)
                ),
            Function = StopColorsToFunction(radial.StopColors, f),
            Extend = (true, true),
        };
        _ = document.AddShading(shading);
        return shading;
    }

    public static FormXObject CreateTransparencyFormXObject(float left, float bottom, float right, float top, IShading shading) => new()
    {
        BBox = (new PointValue(left), new PointValue(bottom), new PointValue(right), new PointValue(top)),
        ResourcesShading = [shading],
        Group = new GroupXObject
        {
            S = "/Transparency",
            I = true,
            CS = "/DeviceGray",
        },
    };

    public static GraphicsStateParameter CreateTransparencyGraphicsStateParameter(Document document, FormXObject g)
    {
        var gstate = new GraphicsStateParameter
        {
            Name = $"gs{document.PdfObjects.Count}",
            SMask = new SoftMask { S = MaskMethods.Luminosity, G = g },
            Ca = 1.0f,
            CA = 1.0f,
            AIS = false,
        };
        _ = document.AddGraphicsStateParameter(gstate);
        return gstate;
    }

    public static float[] ColorToRGB(Color color) => [color.R / 255f, color.G / 255f, color.B / 255f];

    public static float[] ColorToAlpha(Color color) => [color.A / 255f];

    public static IFunction StopColorsToFunction((float Offset, Color StopColor)[] stops, Func<Color, float[]> f)
    {
        Debug.Assert(stops.Length > 0);
        var exponentials = new ExponentialInterpolationFunction[stops.Length];
        var bounds = new float[stops.Length - 1];
        var encode = new float[stops.Length * 2];

        exponentials[0] = new ExponentialInterpolationFunction
        {
            Domain = [0.0f, 1.0f],
            C0 = f(stops[0].StopColor),
            C1 = f(stops[0].StopColor),
            N = 1,
        };
        encode[0] = 0.0f;
        encode[1] = 1.0f;
        for (var i = 0; i < stops.Length - 1; i++)
        {
            var (offset0, color0) = stops[i];
            var (_, color1) = stops[i + 1];
            exponentials[i + 1] = new ExponentialInterpolationFunction
            {
                Domain = [0.0f, 1.0f],
                C0 = f(color0),
                C1 = f(color1),
                N = 1,
            };
            bounds[i] = offset0 / 100.0f;
            encode[i * 2 + 2] = 0.0f;
            encode[i * 2 + 3] = 1.0f;
        }
        return new StitchingFunction
        {
            Domain = [0.0f, 1.0f],
            Functions = exponentials,
            Bounds = bounds,
            Encode = encode,
        };
    }
}
