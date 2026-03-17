using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class OffsetTable : IExportable
{
    public required uint SfntVersion { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required ushort SearchRange { get; init; }
    public required ushort EntrySelector { get; init; }
    public required ushort RangeShift { get; init; }

    public static OffsetTable ReadFrom(Stream stream) => new()
    {
        SfntVersion = stream.ReadUIntByBigEndian(),
        NumberOfTables = stream.ReadUShortByBigEndian(),
        SearchRange = stream.ReadUShortByBigEndian(),
        EntrySelector = stream.ReadUShortByBigEndian(),
        RangeShift = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian(SfntVersion);
        stream.WriteUShortByBigEndian(NumberOfTables);
        stream.WriteUShortByBigEndian(SearchRange);
        stream.WriteUShortByBigEndian(EntrySelector);
        stream.WriteUShortByBigEndian(RangeShift);
    }
}
