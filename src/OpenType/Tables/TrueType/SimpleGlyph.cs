using Mina.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables.TrueType;

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
    public required byte[] Flags { get; init; }
    public required short[] XCoordinates { get; init; }
    public required short[] YCoordinates { get; init; }

    public static SimpleGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = stream.ReadShortByBigEndian();
        var ymin = stream.ReadShortByBigEndian();
        var xmax = stream.ReadShortByBigEndian();
        var ymax = stream.ReadShortByBigEndian();

        var endpoints = Enumerable.Repeat(0, number_of_contours).Select(_ => stream.ReadUShortByBigEndian()).ToArray();
        var instruction_length = stream.ReadUShortByBigEndian();
        var instructions = Enumerable.Repeat(0, instruction_length).Select(_ => stream.ReadUByte()).ToArray();

        var lastpoint = endpoints.Last() + 1;
        var flags = new byte[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i] = stream.ReadUByte();
            if ((flag & (byte)SimpleGlyphFlags.REPEAT_FLAG) != 0)
            {
                var repeat = stream.ReadUByte();
                Enumerable.Repeat(0, repeat).Each(_ => flags[++i] = flag);
            }
        }

        var xcoordinates = new short[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i];
            var issame_or_positive = (flag & (byte)SimpleGlyphFlags.X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR) != 0;
            if ((flag & (byte)SimpleGlyphFlags.X_SHORT_VECTOR) != 0)
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
            var issame_or_positive = (flag & (byte)SimpleGlyphFlags.Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR) != 0;
            if ((flag & (byte)SimpleGlyphFlags.Y_SHORT_VECTOR) != 0)
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
        Instructions.Each(stream.WriteByte);

        Flags.Select(x => (byte)(x
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
