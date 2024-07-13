using Mina.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace PicoPDF.OpenType.Tables;

public class NameTable : IExportable
{
    public required ushort Format { get; init; }
    public required ushort Count { get; init; }
    public required ushort StringOffset { get; init; }
    public required (string Name, NameRecord NameRecord)[] NameRecords { get; init; }
    public ushort LanguageTagCount { get; init; } = 0;
    public (string Name, LanguageTagRecord LanguageTagRecord)[] LanguageTagRecords { get; init; } = [];

    public static NameTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var format = stream.ReadUShortByBigEndian();
        var count = stream.ReadUShortByBigEndian();
        var string_offset = stream.ReadUShortByBigEndian();

        var records = Enumerable.Repeat(0, count)
            .Select(_ => NameRecord.ReadFrom(stream))
            .ToArray();

        ushort lang_tag_count = 0;
        LanguageTagRecord[] tags = [];
        if (format == 1)
        {
            lang_tag_count = stream.ReadUShortByBigEndian();

            tags = Enumerable.Repeat(0, lang_tag_count)
                .Select(_ => LanguageTagRecord.ReadFrom(stream))
                .ToArray();
        }

        var name_records = records
            .Select(x => ((x.PlatformID == (ushort)Platforms.Unicode || x.PlatformID == (ushort)Platforms.Windows ? Encoding.BigEndianUnicode : Encoding.UTF8).GetString(stream.ReadPositionBytes(position + string_offset + x.Offset, x.Length)), x))
            .ToArray();

        var lang_tags = tags
            .Select(x => (Encoding.BigEndianUnicode.GetString(stream.ReadPositionBytes(position + string_offset + x.LanguageTagOffset, x.Length)), x))
            .ToArray();

        return new()
        {
            Format = format,
            Count = count,
            StringOffset = string_offset,
            NameRecords = name_records,
            LanguageTagCount = lang_tag_count,
            LanguageTagRecords = lang_tags,
        };
    }

    public void WriteTo(Stream stream)
    {
        var string_offset = 6 + (/* sizeof(NameRecord) */12 * Count) + (/* sizeof(LanguageTagRecord) */4 * LanguageTagRecords.Length);

        stream.WriteUShortByBigEndian(Format);
        stream.WriteUShortByBigEndian(Count);
        stream.WriteUShortByBigEndian((ushort)string_offset);

        using var strings = new MemoryStream();

        NameRecords.Each(x =>
        {
            var offset = strings.Position;
            strings.Write((x.NameRecord.PlatformID == (ushort)Platforms.Unicode || x.NameRecord.PlatformID == (ushort)Platforms.Windows ? Encoding.BigEndianUnicode : Encoding.UTF8).GetBytes(x.Name));

            stream.WriteUShortByBigEndian(x.NameRecord.PlatformID);
            stream.WriteUShortByBigEndian(x.NameRecord.EncodingID);
            stream.WriteUShortByBigEndian(x.NameRecord.LanguageID);
            stream.WriteUShortByBigEndian(x.NameRecord.NameID);
            stream.WriteUShortByBigEndian((ushort)(strings.Position - offset));
            stream.WriteUShortByBigEndian((ushort)offset);
        });

        if (Format == 1)
        {
            stream.WriteUShortByBigEndian((ushort)LanguageTagRecords.Length);

            LanguageTagRecords.Each(x =>
            {
                var offset = strings.Position;
                strings.Write(Encoding.BigEndianUnicode.GetBytes(x.Name));

                stream.WriteUShortByBigEndian(x.LanguageTagRecord.Length);
                stream.WriteUShortByBigEndian((ushort)offset);
            });
        }

        stream.Write(strings.ToArray());
    }

    public override string ToString() => $"Format={Format}, Count={Count}, StringOffset={StringOffset}";
}
