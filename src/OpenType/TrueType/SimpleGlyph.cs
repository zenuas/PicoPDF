using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.TrueType;

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
        var xmin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var xmax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));

        var endpoints = Enumerable.Range(0, number_of_contours).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray();
        var instruction_length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var instructions = Enumerable.Range(0, instruction_length).Select(_ => (byte)stream.ReadByte()).ToArray();

        var lastpoint = endpoints.Last() + 1;
        var flags = new byte[lastpoint];
        var xcoordinates = new short[lastpoint];
        var ycoordinates = new short[lastpoint];
        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i] = (byte)stream.ReadByte();
            if ((flags[i] & (byte)SimpleGlyphFlags.REPEAT_FLAG) != 0)
            {
                var repeat = stream.ReadByte();
                Enumerable.Range(0, repeat).Each(_ => flags[++i] = flag);
            }
        }

        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i];
            var issame_or_positive = (flag & (byte)SimpleGlyphFlags.X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR) != 0;
            if ((flag & (byte)SimpleGlyphFlags.X_SHORT_VECTOR) != 0)
            {
                xcoordinates[i] = (short)stream.ReadByte();
                if (!issame_or_positive) xcoordinates[i] = (short)-xcoordinates[i];
            }
            else
            {
                xcoordinates[i] = issame_or_positive ? (short)0 : BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            }
        }

        for (var i = 0; i < lastpoint; i++)
        {
            var flag = flags[i];
            var issame_or_positive = (flag & (byte)SimpleGlyphFlags.Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR) != 0;
            if ((flag & (byte)SimpleGlyphFlags.Y_SHORT_VECTOR) != 0)
            {
                ycoordinates[i] = (short)stream.ReadByte();
                if (!issame_or_positive) ycoordinates[i] = (short)-ycoordinates[i];
            }
            else
            {
                ycoordinates[i] = issame_or_positive ? (short)0 : BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
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
}
