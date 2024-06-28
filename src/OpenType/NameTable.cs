using Mina.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.OpenType;

public class NameTable
{
    public required ushort Format { get; init; }
    public required ushort Count { get; init; }
    public required ushort StringOffset { get; init; }
    public required (string Name, NameRecord NameRecord)[] NameRecords { get; init; }

    public static NameTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var format = stream.ReadUShortByBigEndian();
        var count = stream.ReadUShortByBigEndian();
        var string_offset = stream.ReadUShortByBigEndian();

        var records = Enumerable.Range(0, count)
            .Select(_ => NameRecord.ReadFrom(stream))
            .ToArray();

        var name_records = records
            .Select(x => ((x.PlatformID == 0 || x.PlatformID == 3 ? Encoding.BigEndianUnicode : Encoding.UTF8).GetString(stream.ReadPositionBytes(position + string_offset + x.Offset, x.Length)), x))
            .ToArray();

        return new()
        {
            Format = format,
            Count = count,
            StringOffset = string_offset,
            NameRecords = name_records,
        };
    }

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}
