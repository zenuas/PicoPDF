using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType.TrueType;

public class CompositeGlyph : IGlyph
{
    public required short NumberOfContours { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required ushort Flags { get; init; }
    public required ushort GlyphIndex { get; init; }
    public required int Argument1 { get; init; }
    public required int Argument2 { get; init; }

    public static CompositeGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var xmax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));

        var flags = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var glyph_index = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        int arg1 = 0;
        int arg2 = 0;

        var isargs_are_xy_values = (flags & (ushort)CompositeGlyphFlags.ARGS_ARE_XY_VALUES) != 0;
        if ((flags & (ushort)CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS) != 0)
        {
            if (isargs_are_xy_values)
            {
                arg1 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
                arg2 = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
            }
            else
            {
                arg1 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                arg2 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            }
        }
        else
        {
            if (isargs_are_xy_values)
            {
                arg1 = (sbyte)stream.ReadByte();
                arg2 = (sbyte)stream.ReadByte();
            }
            else
            {
                arg1 = stream.ReadByte();
                arg2 = stream.ReadByte();
            }
        }

        return null!;
    }
}
