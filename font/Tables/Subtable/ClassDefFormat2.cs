using Mina.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Subtable;

public class ClassDefFormat2 : ISubtable, IClassDefFormat
{
    public required ushort Format { get; init; }
    public required ushort ClassRangeCount { get; init; }
    public required ClassRangeRecord[] ClassRangeRecords { get; init; }

    public static ClassDefFormat2 ReadFrom(Stream stream)
    {
        var class_range_count = stream.ReadUShortByBigEndian();

        return new()
        {
            Format = 2,
            ClassRangeCount = class_range_count,
            ClassRangeRecords = [.. Lists.Repeat(() => ClassRangeRecord.ReadFrom(stream)).Take(class_range_count)],
        };
    }

    public ushort GetClassValue(uint gid) => ClassRangeRecords.FirstOrDefault(x => x.StartGlyphID <= gid && gid <= x.EndGlyphID)?.Class ?? 0;
}
