using Mina.Binder;
using Mina.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.GlyphSubstitution;

public class CoverageFormat2 : ISubtable, ICoverageFormat
{
    public required ushort Format { get; init; }
    public required ushort RangeCount { get; init; }
    public required (ushort StartGlyphID, ushort EndGlyphID, ushort StartCoverageIndex)[] RangeRecords { get; init; }

    public static CoverageFormat2 ReadFrom(Stream stream)
    {
        var range_count = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 2,
            RangeCount = range_count,
            RangeRecords = [.. Lists.Repeat(() => (stream.ReadUShortByBigEndian(), stream.ReadUShortByBigEndian(), stream.ReadUShortByBigEndian())).Take(range_count)],
        };
    }

    public static readonly ComparerBinder<(ushort StartGlyphID, ushort EndGlyphID, ushort StartCoverageIndex)> RangeComparer = new() { Compare = (a, b) => a.EndGlyphID < b.StartGlyphID ? -1 : a.StartGlyphID > b.EndGlyphID ? 1 : 0 };

    public int? FindOrNull(uint gid)
    {
        var index = RangeRecords.BinarySearch(((ushort)gid, (ushort)gid, (ushort)0), RangeComparer);
        return index < 0 ? null : (int)(gid - RangeRecords[index].StartGlyphID + RangeRecords[index].StartCoverageIndex);
    }
}
