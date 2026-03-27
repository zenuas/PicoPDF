using Mina.Extension;
using OpenType.Outline;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenType.Tables.TrueType;

public class CompositeGlyph : IGlyph
{
    public required short NumberOfContours { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required CompositeGlyphRecord[] CompositeGlyphRecords { get; init; }
    public required ushort InstructionLength { get; init; }
    public required byte[] Instructions { get; init; }

    public IOutline[] ToOutline() => [];

    public static CompositeGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = stream.ReadShortByBigEndian();
        var ymin = stream.ReadShortByBigEndian();
        var xmax = stream.ReadShortByBigEndian();
        var ymax = stream.ReadShortByBigEndian();

        var records = new List<CompositeGlyphRecord>();
        ushort instruction_length = 0;
        byte[]? instructions = null;
        while (true)
        {
            var flags = (CompositeGlyphFlags)stream.ReadUShortByBigEndian();
            var glyph_index = stream.ReadUShortByBigEndian();
            int arg1;
            int arg2;

            var isargs_are_xy_values = flags.HasFlag(CompositeGlyphFlags.ARGS_ARE_XY_VALUES);
            if (flags.HasFlag(CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
            {
                if (isargs_are_xy_values)
                {
                    arg1 = stream.ReadShortByBigEndian();
                    arg2 = stream.ReadShortByBigEndian();
                }
                else
                {
                    arg1 = stream.ReadUShortByBigEndian();
                    arg2 = stream.ReadUShortByBigEndian();
                }
            }
            else
            {
                if (isargs_are_xy_values)
                {
                    arg1 = stream.ReadSByte();
                    arg2 = stream.ReadSByte();
                }
                else
                {
                    arg1 = stream.ReadUByte();
                    arg2 = stream.ReadUByte();
                }
            }

            ushort scale = 0;
            ushort xscale = 0;
            ushort yscale = 0;
            ushort scale01 = 0;
            ushort scale10 = 0;
            if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_SCALE))
            {
                scale = stream.ReadUShortByBigEndian();
            }
            else if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
            {
                xscale = stream.ReadUShortByBigEndian();
                yscale = stream.ReadUShortByBigEndian();
            }
            else if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
            {
                xscale = stream.ReadUShortByBigEndian();
                scale01 = stream.ReadUShortByBigEndian();
                scale10 = stream.ReadUShortByBigEndian();
                yscale = stream.ReadUShortByBigEndian();
            }
            records.Add(new()
            {
                Flags = flags,
                GlyphIndex = glyph_index,
                Argument1 = arg1,
                Argument2 = arg2,
                Scale = scale,
                XScale = xscale,
                YScale = yscale,
                Scale01 = scale01,
                Scale10 = scale10,
            });

            if (!flags.HasFlag(CompositeGlyphFlags.MORE_COMPONENTS))
            {
                if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_INSTRUCTIONS))
                {
                    instruction_length = stream.ReadUShortByBigEndian();
                    instructions = [.. Lists.Repeat(stream.ReadUByte).Take(instruction_length)];
                }
                break;
            }
        }

        return new()
        {
            NumberOfContours = number_of_contours,
            XMin = xmin,
            YMin = ymin,
            XMax = xmax,
            YMax = ymax,
            CompositeGlyphRecords = [.. records],
            InstructionLength = instruction_length,
            Instructions = instructions ?? [],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteShortByBigEndian(NumberOfContours);
        stream.WriteShortByBigEndian(XMin);
        stream.WriteShortByBigEndian(YMin);
        stream.WriteShortByBigEndian(XMax);
        stream.WriteShortByBigEndian(YMax);

        foreach (var glyph in CompositeGlyphRecords)
        {
            stream.WriteUShortByBigEndian((ushort)glyph.Flags);
            stream.WriteUShortByBigEndian(glyph.GlyphIndex);

            if (glyph.Flags.HasFlag(CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
            {
                stream.WriteUShortByBigEndian((ushort)glyph.Argument1);
                stream.WriteUShortByBigEndian((ushort)glyph.Argument2);
            }
            else
            {
                stream.WriteByte((byte)glyph.Argument1);
                stream.WriteByte((byte)glyph.Argument2);
            }

            if (glyph.Flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_SCALE))
            {
                stream.WriteUShortByBigEndian(glyph.Scale);
            }
            else if (glyph.Flags.HasFlag(CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
            {
                stream.WriteUShortByBigEndian(glyph.XScale);
                stream.WriteUShortByBigEndian(glyph.YScale);
            }
            else if (glyph.Flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
            {
                stream.WriteUShortByBigEndian(glyph.XScale);
                stream.WriteUShortByBigEndian(glyph.Scale01);
                stream.WriteUShortByBigEndian(glyph.Scale10);
                stream.WriteUShortByBigEndian(glyph.YScale);
            }
        }

        if (Instructions.Length > 0)
        {
            stream.WriteUShortByBigEndian((ushort)Instructions.Length);
            stream.Write(Instructions);
        }
    }
}
