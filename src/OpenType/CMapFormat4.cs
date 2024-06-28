﻿using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType;

public class CMapFormat4 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required ushort Length { get; init; }
    public required ushort Language { get; init; }
    public required ushort SegCountX2 { get; init; }
    public required ushort SearchRange { get; init; }
    public required ushort EntrySelector { get; init; }
    public required ushort RangeShift { get; init; }
    public required ushort[] EndCode { get; init; }
    public required ushort ReservedPad { get; init; }
    public required ushort[] StartCode { get; init; }
    public required short[] IdDelta { get; init; }
    public required ushort[] IdRangeOffsets { get; init; }
    public required ushort[] GlyphIdArray { get; init; }

    public static CMapFormat4 ReadFrom(Stream stream)
    {
        var length = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var language = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        var seg_count_x2 = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        var seg_count = seg_count_x2 / 2;
        var glyph_count = (length - (16 + (8 * seg_count))) / 2;

        return new()
        {
            Format = 4,
            Length = length,
            Language = language,
            SegCountX2 = seg_count_x2,
            SearchRange = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            EntrySelector = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            RangeShift = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            EndCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            ReservedPad = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            StartCode = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            IdDelta = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            IdRangeOffsets = Lists.RangeTo(0, seg_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
            GlyphIdArray = Lists.RangeTo(0, glyph_count - 1).Select(_ => BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2))).ToArray(),
        };
    }

    public static CMapFormat4 CreateFormat(Dictionary<char, ushort> char_gid)
    {
        var start_ends = CreateStartEnds(char_gid);
        var seg_count = start_ends.Length;
        var serach_range = Math.Pow(2, Math.Floor(Math.Log2(seg_count)));

        return new()
        {
            Format = 4,
            Length = 0,
            Language = 0,
            SegCountX2 = (ushort)(seg_count * 2),
            SearchRange = (ushort)serach_range,
            EntrySelector = (ushort)Math.Log2(serach_range / 2),
            RangeShift = (ushort)((seg_count * 2) - serach_range),
            EndCode = start_ends.Select(x => (ushort)x.End).ToArray(),
            ReservedPad = 0,
            StartCode = start_ends.Select(x => (ushort)x.Start).ToArray(),
            IdDelta = start_ends.Select(x => (short)(char_gid[x.Start] - x.Start)).ToArray(),
            IdRangeOffsets = Lists.Repeat((ushort)0).Take(seg_count).ToArray(),
            GlyphIdArray = [],
        };
    }

    public static (char Start, char End)[] CreateStartEnds(Dictionary<char, ushort> char_gid) => CreateStartEnds(char_gid.Keys.Order().ToArray(), char_gid);

    public static (char Start, char End)[] CreateStartEnds(Span<char> chars, Dictionary<char, ushort> char_gid)
    {
        var start = chars[0];
        var gid = char_gid[chars[0]];
        if (chars.Length == 1) return [(start, start)];

        for (var i = 1; i < chars.Length; i++)
        {
            if (start + i == chars[i] && gid + i == char_gid[chars[i]]) continue;
            return [(start, chars[i - 1]), .. CreateStartEnds(chars[i..], char_gid)];
        }
        return [(start, chars[^1])];
    }
}
