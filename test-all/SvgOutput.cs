using Mina.Extension;
using OpenType;
using OpenType.Outline;
using System;
using System.Linq;
using System.Text;

namespace PicoPDF.TestAll;

public static class SvgOutput
{
    public static void Output(IOpenTypeFont font, string str)
    {
        var cids = str.ToUtf32CharArray();
        var canvas = cids.Select(font.CharToGID)
            .Select(gid => GetCanvasSize(font, gid))
            .Aggregate((a, b) => (a.Width + b.Width, Math.Max(a.Ascent, b.Ascent), Math.Min(a.Descent, b.Descent)));

        var r = 1 / (canvas.Ascent - canvas.Descent) * 100;
        var left = 0f;
        var baseline = canvas.Ascent * r;
        Console.WriteLine($"""<svg width="{canvas.Width * r}" height="{(canvas.Ascent - canvas.Descent) * r}" xmlns="http://www.w3.org/2000/svg">""");
        foreach (var cid in cids)
        {
            var c = char.ConvertFromUtf32(cid);
            var gid = font.CharToGID(cid);
            var outlines = font.GIDToOutline(gid);
            var width = 0.0f;

            foreach (var outline in outlines)
            {
                switch (outline)
                {
                    case Surface surface:
                        {
                            var start = surface.Edges.First().Start;
                            var d = new StringBuilder($"M {left + (start.X * r)} {baseline - (start.Y * r)}");
                            foreach (var edge in surface.Edges)
                            {
                                switch (edge)
                                {
                                    case Line line:
                                        d.Append($" L {left + (line.End.X * r)} {baseline - (line.End.Y * r)}");
                                        break;
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 1:
                                        {
                                            var cp = bezier.ControlPoint[0];
                                            d.Append($" Q {left + (cp.X * r)} {baseline - (cp.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 2:
                                        {
                                            var cp1 = bezier.ControlPoint[0];
                                            var cp2 = bezier.ControlPoint[1];
                                            d.Append($" C {left + (cp1.X * r)} {baseline - (cp1.Y * r)}, {left + (cp2.X * r)} {baseline - (cp2.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                }
                            }
                            Console.WriteLine($"""    <path d="{d}" stroke="black" fill="transparent" />""");
                            width = (surface.XMax - surface.XMin) * r;
                            break;
                        }
                }
            }
            left += width;
        }
        Console.WriteLine($"""</svg>""");
    }

    public static (float Width, float Ascent, float Descent) GetCanvasSize(IOpenTypeFont font, uint gid)
    {
        foreach (var outline in font.GIDToOutline(gid))
        {
            switch (outline)
            {
                case Surface surface:
                    return (surface.XMax + Math.Abs(surface.XMin), surface.YMax, surface.YMin);
            }
        }
        return (0, 0, 0);
    }
}
