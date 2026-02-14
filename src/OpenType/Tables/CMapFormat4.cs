using Mina.Binder;
using Mina.Extension;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

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
        var length = stream.ReadUShortByBigEndian();
        var language = stream.ReadUShortByBigEndian();
        var seg_count_x2 = stream.ReadUShortByBigEndian();

        var seg_count = seg_count_x2 / 2;
        var glyph_count = (length - (16 + (8 * seg_count))) / 2;

        return new()
        {
            Format = 4,
            Length = length,
            Language = language,
            SegCountX2 = seg_count_x2,
            SearchRange = stream.ReadUShortByBigEndian(),
            EntrySelector = stream.ReadUShortByBigEndian(),
            RangeShift = stream.ReadUShortByBigEndian(),
            EndCode = [.. Enumerable.Repeat(0, seg_count).Select(_ => stream.ReadUShortByBigEndian())],
            ReservedPad = stream.ReadUShortByBigEndian(),
            StartCode = [.. Enumerable.Repeat(0, seg_count).Select(_ => stream.ReadUShortByBigEndian())],
            IdDelta = [.. Enumerable.Repeat(0, seg_count).Select(_ => stream.ReadShortByBigEndian())],
            IdRangeOffsets = [.. Enumerable.Repeat(0, seg_count).Select(_ => stream.ReadUShortByBigEndian())],
            GlyphIdArray = [.. Enumerable.Repeat(0, glyph_count).Select(_ => stream.ReadUShortByBigEndian())],
        };
    }

    public static CMapFormat4 CreateFormat(Dictionary<char, ushort> char_gid)
    {
        var chars = char_gid.Keys.Order().ToArray();
        var start_ends = CreateStartEnds(chars, char_gid);
        var seg_count = start_ends.Length;
        var serach_range = Math.Pow(2, Math.Floor(Math.Log2(seg_count))) * 2;

        return new()
        {
            Format = 4,
            Length = (ushort)((sizeof(ushort) * 8) + (seg_count * sizeof(ushort) * 4) + (chars.Length * sizeof(ushort))),
            Language = 0,
            SegCountX2 = (ushort)(seg_count * 2),
            SearchRange = (ushort)serach_range,
            EntrySelector = (ushort)Math.Log2(serach_range / 2),
            RangeShift = (ushort)((seg_count * 2) - serach_range),
            EndCode = [.. start_ends.Select(x => (ushort)x.End)],
            ReservedPad = 0,
            StartCode = [.. start_ends.Select(x => (ushort)x.Start)],
            IdDelta = [.. start_ends.Select(x => (short)(x.Start == 0xFFFF ? 1 : char_gid[x.Start] - x.Start))],
            IdRangeOffsets = [.. Lists.Repeat((ushort)0).Take(seg_count)],
            GlyphIdArray = [.. chars.Select(x => char_gid[x])],
        };
    }

    public static (char Start, char End)[] CreateStartEnds(Span<char> chars, Dictionary<char, ushort> char_gid)
    {
        if (chars.Length == 0) return [];
        var start = chars[0];
        var gid = char_gid[chars[0]];
        if (chars.Length == 1) return [(start, start), ((char)0xFFFF, (char)0xFFFF)];

        for (var i = 1; i < chars.Length; i++)
        {
            if (start + i == chars[i] && gid + i == char_gid[chars[i]]) continue;
            return [(start, chars[i - 1]), .. CreateStartEnds(chars[i..], char_gid)];
        }
        return [(start, chars[^1]), ((char)0xFFFF, (char)0xFFFF)];
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Length);
        stream.WriteUShortByBigEndian(Language);
        stream.WriteUShortByBigEndian(SegCountX2);
        stream.WriteUShortByBigEndian(SearchRange);
        stream.WriteUShortByBigEndian(EntrySelector);
        stream.WriteUShortByBigEndian(RangeShift);
        EndCode.Each(stream.WriteUShortByBigEndian);
        stream.WriteUShortByBigEndian(ReservedPad);
        StartCode.Each(stream.WriteUShortByBigEndian);
        IdDelta.Each(stream.WriteShortByBigEndian);
        IdRangeOffsets.Each(stream.WriteUShortByBigEndian);
        GlyphIdArray.Each(stream.WriteUShortByBigEndian);
    }

    public List<(int Start, int End)> CreateRange()
    {
        var range = new List<(int Start, int End)>();
        _ = EndCode.Aggregate(0, (acc, x) =>
        {
            range.Add((acc, x));
            return x + 1;
        });
        return range;
    }

    public static readonly ComparerBinder<(int Start, int End)> RangeComparer = new() { Compare = (a, b) => a.End < b.End ? -1 : a.Start > b.Start ? 1 : 0 };

    public Func<int, uint> CreateCharToGID()
    {
        var range = CreateRange();
        var cache = new Dictionary<char, uint>();

        return (c) =>
        {
            if (cache.TryGetValue((char)c, out var gid)) return gid;

            var seg = range.BinarySearch((c, c), RangeComparer);
            if (seg < 0) return cache[(char)c] = 0;
            var start = StartCode[seg];
            if (c < start) return cache[(char)c] = 0;

            var idrange = IdRangeOffsets[seg];
            if (idrange == 0) return cache[(char)c] = (uint)((c + IdDelta[seg]) & 0xFFFF);

            var gindex = (idrange / 2) + c - start - (SegCountX2 / 2) + seg;
            return cache[(char)c] = (uint)((GlyphIdArray[gindex] + IdDelta[seg]) & 0xFFFF);
        };
    }
}
