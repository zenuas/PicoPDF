﻿using Mina.Extension;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.TrueType;

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

    public static CompositeGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymin = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var xmax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));
        var ymax = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));

        var records = new List<CompositeGlyphRecord>();
        ushort instruction_length = 0;
        byte[]? instructions;
        while (true)
        {
            var flags = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            var glyph_index = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            int arg1;
            int arg2;

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

            ushort scale = 0;
            ushort xscale = 0;
            ushort yscale = 0;
            ushort scale01 = 0;
            ushort scale10 = 0;
            if ((flags & (ushort)CompositeGlyphFlags.WE_HAVE_A_SCALE) != 0)
            {
                scale = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            }
            else if ((flags & (ushort)CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE) != 0)
            {
                xscale = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                yscale = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            }
            else if ((flags & (ushort)CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO) != 0)
            {
                xscale = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                scale01 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                scale10 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                yscale = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
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

            if ((flags & (ushort)CompositeGlyphFlags.MORE_COMPONENTS) == 0)
            {
                instruction_length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
                instructions = Enumerable.Range(0, instruction_length).Select(_ => (byte)stream.ReadByte()).ToArray();
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
}
