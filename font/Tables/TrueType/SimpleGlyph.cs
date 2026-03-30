using Mina.Extension;
using OpenType.Outline;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenType.Tables.TrueType;

public class SimpleGlyph : IGlyph
{
    public required short NumberOfContours { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required ushort[] EndPointsOfContours { get; init; }
    public required ushort InstructionLength { get; init; }
    public required byte[] Instructions { get; init; }
    public required SimpleGlyphFlags[] Flags { get; init; }
    public required short[] XCoordinates { get; init; }
    public required short[] YCoordinates { get; init; }

    public IOutline[] ToOutline()
    {
        var outlines = new IOutline[NumberOfContours];
        var xcoordinates = AbsoluteCoordinates(XCoordinates);
        var ycoordinates = AbsoluteCoordinates(YCoordinates);
        for (var (i, start) = (0, 0); i < NumberOfContours; start = EndPointsOfContours[i++] + 1)
        {
            outlines[i] = CreateSurface(start, EndPointsOfContours[i], Flags, xcoordinates, ycoordinates, XMin, YMin, XMax, YMax);
        }
        return outlines;
    }

    public static Surface CreateSurface(int start, int end, SimpleGlyphFlags[] flags, int[] xcoordinates, int[] ycoordinates, float xmin, float ymin, float xmax, float ymax)
    {
        var edges = new List<IEdge>();
        var prev = new Vector2(xcoordinates[start], ycoordinates[start]);
        for (var i = start + 1; i <= end; i++)
        {
            var next = new Vector2(xcoordinates[i], ycoordinates[i]);
            if (flags[i].HasFlag(SimpleGlyphFlags.ON_CURVE_POINT))
            {
                edges.Add(new Line { Start = prev, End = next });
                prev = next;
            }
            else if (i + 1 <= end)
            {
                var next2 = new Vector2(xcoordinates[i + 1], ycoordinates[i + 1]);
                if (flags[i + 1].HasFlag(SimpleGlyphFlags.ON_CURVE_POINT))
                {
                    edges.Add(new BezierCurves { Start = prev, End = next2, ControlPoint = [next] });
                    prev = next2;
                    i++;
                }
                else
                {
                    var complement_point = (next2 + next) / 2;
                    edges.Add(new BezierCurves { Start = prev, End = complement_point, ControlPoint = [next] });
                    prev = complement_point;
                }
            }
            else
            {
                edges.Add(new BezierCurves { Start = prev, End = edges.First().Start, ControlPoint = [next] });
                return new() { XMin = xmin, YMin = ymin, XMax = xmax, YMax = ymax, Edges = [.. edges] };
            }
        }
        if (edges.First().Start != edges.Last().End) edges.Add(new Line { Start = prev, End = edges.First().Start });
        return new() { XMin = xmin, YMin = ymin, XMax = xmax, YMax = ymax, Edges = [.. edges] };
    }

    public static int[] AbsoluteCoordinates(short[] relative_coordinates)
    {
        var absolute_coordinates = new int[relative_coordinates.Length];
        absolute_coordinates[0] = relative_coordinates[0];
        for (var i = 1; i < relative_coordinates.Length; i++)
        {
            absolute_coordinates[i] = absolute_coordinates[i - 1] + relative_coordinates[i];
        }
        return absolute_coordinates;
    }

    public static SimpleGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = stream.ReadShortByBigEndian();
        var ymin = stream.ReadShortByBigEndian();
        var xmax = stream.ReadShortByBigEndian();
        var ymax = stream.ReadShortByBigEndian();

        var endpoints = Lists.Repeat(stream.ReadUShortByBigEndian).Take(number_of_contours).ToArray();
        var instruction_length = stream.ReadUShortByBigEndian();
        var instructions = Lists.Repeat(stream.ReadUByte).Take(instruction_length).ToArray();

        var lastpoint = endpoints.Last() + 1;
        var flags = new SimpleGlyphFlags[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i] = (SimpleGlyphFlags)stream.ReadUByte();
            if (flag.HasFlag(SimpleGlyphFlags.REPEAT_FLAG))
            {
                var repeat = stream.ReadUByte();
                for (var j = 0; j < repeat; j++) flags[++i] = flag;
            }
        }

        var xcoordinates = new short[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i];
            var issame_or_positive = flag.HasFlag(SimpleGlyphFlags.X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR);
            if (flag.HasFlag(SimpleGlyphFlags.X_SHORT_VECTOR))
            {
                xcoordinates[i] = stream.ReadUByte();
                if (!issame_or_positive) xcoordinates[i] = (short)-xcoordinates[i];
            }
            else
            {
                xcoordinates[i] = issame_or_positive ? (short)0 : stream.ReadShortByBigEndian();
            }
        }

        var ycoordinates = new short[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i];
            var issame_or_positive = flag.HasFlag(SimpleGlyphFlags.Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR);
            if (flag.HasFlag(SimpleGlyphFlags.Y_SHORT_VECTOR))
            {
                ycoordinates[i] = stream.ReadUByte();
                if (!issame_or_positive) ycoordinates[i] = (short)-ycoordinates[i];
            }
            else
            {
                ycoordinates[i] = issame_or_positive ? (short)0 : stream.ReadShortByBigEndian();
            }
        }

        return new()
        {
            NumberOfContours = number_of_contours,
            XMin = xmin,
            YMin = ymin,
            XMax = xmax,
            YMax = ymax,
            EndPointsOfContours = endpoints,
            InstructionLength = instruction_length,
            Instructions = instructions,
            Flags = flags,
            XCoordinates = xcoordinates,
            YCoordinates = ycoordinates,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteShortByBigEndian((short)EndPointsOfContours.Length);
        stream.WriteShortByBigEndian(XMin);
        stream.WriteShortByBigEndian(YMin);
        stream.WriteShortByBigEndian(XMax);
        stream.WriteShortByBigEndian(YMax);
        EndPointsOfContours.Each(stream.WriteUShortByBigEndian);
        stream.WriteUShortByBigEndian((ushort)Instructions.Length);
        stream.Write(Instructions);

        Flags.Select(x => (byte)((byte)x
                & ~(byte)SimpleGlyphFlags.REPEAT_FLAG
                & ~(byte)SimpleGlyphFlags.X_SHORT_VECTOR
                & ~(byte)SimpleGlyphFlags.X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR
                & ~(byte)SimpleGlyphFlags.Y_SHORT_VECTOR
                & ~(byte)SimpleGlyphFlags.Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR
                & 0xFF))
            .Each(stream.WriteByte);

        XCoordinates.Each(stream.WriteShortByBigEndian);
        YCoordinates.Each(stream.WriteShortByBigEndian);
    }
}
