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
        OutputSvg(font, [.. args.Select(arg => arg.ToUtf32CharArray().Select(x => (x, font.CharToGID(x))).ToArray())]);
        Output.Flush();
    }

    public void OutputSvg(IOpenTypeFont font, (int Char, uint GID)[][] cidss)
    {
        var top = 0f;
        var max_width = 0f;
        using var mem = new MemoryStream();
        using var writer = new StreamWriter(mem);
        foreach (var cids in cidss)
        {
            var (width, height) = OutputPath(font, writer, top, cids);
            top += height;
            max_width = Math.Max(width, max_width);
        }
        writer.Flush();
        Output.WriteLine($"""<svg width="{max_width}" height="{top}" xmlns="http://www.w3.org/2000/svg">""");
        Output.Write(Encoding.UTF8.GetString(mem.ToArray()));
        Output.WriteLine("</svg>");
    }

    public (float Width, float Height) OutputPath(IOpenTypeFont font, TextWriter writer, float top, (int Char, uint GID)[] cids)
    {
        var surfacess = cids.Select(x => font.GIDToOutline(x.GID)).ToArray();
        var total_width = cids.Select(x => font.GetAdvanceWidth(x.GID)).Sum();
        var ascent = font.HorizontalHeader.Ascender;
        var descent = font.HorizontalHeader.Descender;

        var r = 1f / font.FontHeader.UnitsPerEm * Point;
        var left = 0f;
        var baseline = top + (ascent * r);
        for (var i = 0; i < cids.Length; i++)
        {
            writer.WriteLine($"    <!-- {char.ConvertFromUtf32(cids[i].Char)} -->");
            var d = new StringBuilder();
            var c = new StringBuilder();
            foreach (var surface in surfacess[i].Where(x => x.Edges.Length > 0))
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

                        case BezierCurve bezier when bezier.ControlPoint.Length == 1:
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

                        case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                            {
                                var cp1 = bezier.ControlPoint[0];
                                var cp2 = bezier.ControlPoint[1];
                                if (JointPoint > 0)
                                {
                                    _ = c.AppendLine($"""    <circle cx="{left + (cp1.X * r)}" cy="{baseline - (cp1.Y * r)}" r="{JointPoint}" fill="red" />""");
                                    _ = c.AppendLine($"""    <circle cx="{left + (cp2.X * r)}" cy="{baseline - (cp2.Y * r)}" r="{JointPoint}" fill="red" />""");
                                    _ = c.AppendLine($"""    <circle cx="{left + (bezier.End.X * r)}" cy="{baseline - (bezier.End.Y * r)}" r="{JointPoint}" fill="{(bezier.ComplementPoint ? "green" : "blue")}" />""");
                                }
                                _ = d.AppendLine($"          C {left + (cp1.X * r)} {baseline - (cp1.Y * r)}, {left + (cp2.X * r)} {baseline - (cp2.Y * r)}, {left + (bezier.End.X * r)} {baseline - (bezier.End.Y * r)}");
                                break;
                            }
                    }
                }
                _ = d.Append("          Z");
            }
            writer.WriteLine($"""    <path stroke="{ColorToHex(Stroke)}" fill="{ColorToHex(Fill)}" fill-rule="evenodd" """);
            writer.WriteLine($"""       d="{d}" />""");
            writer.Write(c);
            left += font.GetAdvanceWidth(cids[i].GID) * r;
        }
        if (Debug)
        {
            writer.WriteLine($"    <!-- baseline -->");
            writer.WriteLine($"""    <line x1="0" y1="{baseline}" x2="{total_width * r}" y2="{baseline}" stroke="red" />""");
        }
        return (total_width * r, (ascent - descent) * r);
    }

    public static string ColorToHex(Color color) => color == Color.Transparent ? "transparent" : $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
