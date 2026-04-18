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
            OutputSvg(font, [.. arg.ToUtf32CharArray()]);
        }
        Output.Flush();
    }

    public void OutputSvg(IOpenTypeFont font, int[] cids)
    {
        var gids = cids.Select(font.CharToGID).ToArray();
        var outlines = gids.Select(font.GIDToOutline).ToArray();
        var total_width = outlines.Select(x => GetCanvasWidth(x).Width).Sum();
        var ascent = font.HorizontalHeader.Ascender;
        var descent = font.HorizontalHeader.Descender;

        var r = 1f / (ascent - descent) * Point;
        var left = 0f;
        var baseline = ascent * r;
        Output.WriteLine($"""<svg width="{total_width * r}" height="{(ascent - descent) * r}" xmlns="http://www.w3.org/2000/svg">""");
        for (var i = 0; i < cids.Length; i++)
        {
            var (gid_width, gid_left) = GetCanvasWidth(outlines[i]);
            left -= gid_left * r;
            Output.WriteLine($"    <!-- {char.ConvertFromUtf32(cids[i])} -->");
            var d = new StringBuilder();
            var c = new StringBuilder();
            foreach (var outline in font.GIDToOutline(gids[i]))
            {
                switch (outline)
                {
                    case Surface surface when surface.Edges.Length > 0:
                        {
                            var start = surface.Edges.First().Start;
                            if (JointPoint > 0) _ = c.AppendLine($"""    <circle cx="{left + (start.X * r)}" cy="{baseline - (start.Y * r)}" r="{JointPoint}" fill="blue" />""");
                            _ = d.AppendLine();
                            _ = d.AppendLine($"          M {left + (start.X * r)} {baseline - (start.Y * r)}");
                            foreach (var edge in surface.Edges)
                            {
                                switch (edge)
                                {
                                    case Line line:
                                        if (JointPoint > 0) _ = c.AppendLine($"""    <circle cx="{left + (line.End.X * r)}" cy="{baseline - (line.End.Y * r)}" r="{JointPoint}" fill="blue" />""");
                                        _ = d.AppendLine($"          L {left + (line.End.X * r)} {baseline - (line.End.Y * r)}");
                                        break;
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 1:
                                        {
                                            var cp = bezier.ControlPoint[0];
                                            if (JointPoint > 0)
                                            {
                                                _ = c.AppendLine($"""    <circle cx="{left + (cp.X * r)}" cy="{baseline - (cp.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                _ = c.AppendLine($"""    <circle cx="{left + (bezier.End.X * r)}" cy="{baseline - (bezier.End.Y * r)}" r="{JointPoint}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                            }
                                            _ = d.AppendLine($"          Q {left + (cp.X * r)} {baseline - (cp.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                    case BezierCurves bezier when bezier.ControlPoint.Length == 2:
                                        {
                                            var cp1 = bezier.ControlPoint[0];
                                            var cp2 = bezier.ControlPoint[1];
                                            if (JointPoint > 0)
                                            {
                                                _ = c.AppendLine($"""    <circle cx="{left + (cp1.X * r)}" cy="{baseline - (cp1.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                _ = c.AppendLine($"""    <circle cx="{left + (cp2.X * r)}" cy="{baseline - (cp2.Y * r)}" r="{JointPoint}" fill="red" />""");
                                                _ = c.AppendLine($"""    <circle cx="{left + (bezier.End.X * r)}" cy="{left + (bezier.End.Y * r)}" r="{JointPoint}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                            }
                                            _ = d.AppendLine($"          C {left + (cp1.X * r)} {baseline - (cp1.Y * r)}, {left + (cp2.X * r)} {baseline - (cp2.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                            break;
                                        }
                                }
                            }
                            _ = d.Append("          Z");
                            break;
                        }
                }
            }
            Output.WriteLine($"""    <path stroke="{ColorToHex(Stroke)}" fill="{ColorToHex(Fill)}" fill-rule="evenodd" """);
            Output.WriteLine($"""       d="{d}" />""");
            Output.Write(c);
            left += (gid_width + gid_left) * r;
        }
        if (Debug)
        {
            Output.WriteLine($"    <!-- baseline -->");
            Output.WriteLine($"""    <line x1="0" y1="{baseline}" x2="{total_width * r}" y2="{baseline}" stroke="red" />""");
        }
        Output.WriteLine("</svg>");
    }

    public static (float Width, float Left) GetCanvasWidth(IOutline[] outlines)
    {
        foreach (var outline in outlines)
        {
            switch (outline)
            {
                case Surface surface:
                    return (surface.XMax - surface.XMin, surface.XMin);
            }
        }
        return (0, 0);
    }

    public static string ColorToHex(Color color) => color == Color.Transparent ? "transparent" : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
