using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class OffsetTable
{
    public required uint Version { get; init; }
    public required ushort NumberOfTables { get; init; }
    public required ushort SearchRange { get; init; }
    public required ushort EntrySelector { get; init; }
    public required ushort RangeShift { get; init; }

    public static OffsetTable ReadFrom(Stream stream) => new()
    {
        Version = stream.ReadUIntByBigEndian(),
        NumberOfTables = stream.ReadUShortByBigEndian(),
        SearchRange = stream.ReadUShortByBigEndian(),
        EntrySelector = stream.ReadUShortByBigEndian(),
        RangeShift = stream.ReadUShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian(Version);
        stream.WriteUShortByBigEndian(NumberOfTables);
        stream.WriteUShortByBigEndian(SearchRange);
        stream.WriteUShortByBigEndian(EntrySelector);
        stream.WriteUShortByBigEndian(RangeShift);
    }
}
