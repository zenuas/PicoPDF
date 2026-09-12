using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Common;

public class ScriptTable
{
    public required Offset16 DefaultLangSysOffset { get; init; }
    public required ushort LangSysCount { get; init; }
    public required LanguageSystemRecord[] LangSysRecords { get; init; }

    public static ScriptTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var default_lang_sys_offset = stream.ReadOffset16();
        var lang_sys_count = stream.ReadUShortByBigEndian();

        var lang_sys_records_offset = stream.Position;

        return new()
        {
            DefaultLangSysOffset = default_lang_sys_offset,
            LangSysCount = lang_sys_count,
            LangSysRecords = [.. Lists.Sequence(lang_sys_records_offset, LanguageSystemRecord.SizeOf()).Select(x => LanguageSystemRecord.ReadFrom(stream.SeekTo(x), position)).Take(lang_sys_count)],
        };
    }

    public int SizeOf() => DefaultLangSysOffset.SizeOf() + LangSysCount.SizeOf() + (LanguageSystemRecord.SizeOf() * LangSysRecords.Length);
}
