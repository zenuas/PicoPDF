using Mina.Command;
using Mina.Extension;
using OpenType;
using OpenType.Outline;
using System;
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

    public override void Run(string[] args)
    {
        var fontreg = CreateFontRegister();
        var font = fontreg.LoadComplete(Font);
        foreach (var arg in args)
        {
            OutputSvg(font, arg);
        }
    }

    public void OutputSvg(IOpenTypeFont font, string str)
    {
        var cids = str.ToUtf32CharArray();
        var canvas = cids.Select(font.CharToGID)
            .Select(gid => GetCanvasSize(font, gid))
            .Aggregate((a, b) => (a.Width + b.Width, 0, Math.Max(a.Ascent, b.Ascent), Math.Min(a.Descent, b.Descent)));

        var r = 1 / (canvas.Ascent - canvas.Descent) * Point;
        var left = 0f;
        var baseline = canvas.Ascent * r;
        Output.WriteLine($"""<svg width="{canvas.Width * r}" height="{(canvas.Ascent - canvas.Descent) * r}" xmlns="http://www.w3.org/2000/svg">""");
        foreach (var cid in cids)
        {
            var gid = font.CharToGID(cid);
            left -= Math.Min(0, GetCanvasSize(font, gid).Left) * r;
            Output.WriteLine($"    <!-- {char.ConvertFromUtf32(cid)} -->");
            var d = new StringBuilder();
            var c = new StringBuilder();
            foreach (var outline in font.GIDToOutline(gid))
            {
                switch (outline)
                {
                    case Surface surface:
                        {
                            var start = surface.Edges.First().Start;
                            if (JointPoint > 0) c.AppendLine($"""    <circle cx="{left + (start.X * r)}" cy="{baseline - (start.Y * r)}" r="{JointPoint}" fill="blue" />""");
                            d.AppendLine();
                            d.AppendLine($"          M {left + (start.X * r)} {baseline - (start.Y * r)}");
                            foreach (var edge in surface.Edges)
                            {
                                switch (edge)
                                {
                                    case Line line:
                                        if (JointPoint > 0) c.AppendLine($"""    <circle cx="{left + (line.End.X * r)}" cy="{baseline - (line.End.Y * r)}" r="{JointPoint}" fill="blue" />""");
                                        d.AppendLine($"          L {left + (line.End.X * r)} {baseline - (line.End.Y * r)}");
                                        break;
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 1:
                                        {
                                            var cp = bezier.ControlPoint[0];
                                            if (JointPoint > 0)
                                            {
                                                c.AppendLine($"""    <circle cx="{left + (cp.X * r)}" cy="{baseline - (cp.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                c.AppendLine($"""    <circle cx="{left + (bezier.End.X * r)}" cy="{baseline - (bezier.End.Y * r)}" r="{JointPoint}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                            }
                                            d.AppendLine($"          Q {left + (cp.X * r)} {baseline - (cp.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 2:
                                        {
                                            var cp1 = bezier.ControlPoint[0];
                                            var cp2 = bezier.ControlPoint[1];
                                            if (JointPoint > 0)
                                            {
                                                c.AppendLine($"""    <circle cx="{left + (cp1.X * r)}" cy="{baseline - (cp1.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                c.AppendLine($"""    <circle cx="{left + (cp2.X * r)}" cy="{baseline - (cp2.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                c.AppendLine($"""    <circle cx="{left + (bezier.End.X * r)}" cy="{left + (bezier.End.Y * r)}" r="{JointPoint}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                            }
                                            d.AppendLine($"          C {left + (cp1.X * r)} {baseline - (cp1.Y * r)}, {left + (cp2.X * r)} {baseline - (cp2.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                }
                            }
                            d.Append("          Z");
                            break;
                        }
                }
            }
            Output.WriteLine($"""    <path stroke="{ColorToHex(Stroke)}" fill="{ColorToHex(Fill)}" fill-rule="evenodd" """);
            Output.WriteLine($"""       d="{d}" />""");
            Output.Write(c);
            left += GetCanvasSize(font, gid).Width * r;
        }
        if (Debug)
        {
            Output.WriteLine($"    <!-- baseline -->");
            Output.WriteLine($"""    <line x1="0" y1="{baseline}" x2="{canvas.Width * r}" y2="{baseline}" stroke="red" />""");
        }
        Output.WriteLine("</svg>");
        Output.Flush();
    }

    public static (float Width, float Left, float Ascent, float Descent) GetCanvasSize(IOpenTypeFont font, uint gid)
    {
        foreach (var outline in font.GIDToOutline(gid))
        {
            switch (outline)
            {
                case Surface surface:
                    return (surface.XMax + (surface.XMin < 0 ? -surface.XMin : 0), surface.XMin, surface.YMax, surface.YMin);
            }
        }
        return (0, 0, 0, 0);
    }

    public static string ColorToHex(Color color) => color == Color.Transparent ? "transparent" : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
