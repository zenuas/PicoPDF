using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType.Tables.Common;

public class ScriptTableRecord
{
    public required Offset16 DefaultLangSysOffset { get; init; }
    public required ushort LangSysCount { get; init; }
    public required (string LangSysTag, Offset16 LangSysOffset, LanguageSystemTableRecord LanguageSystemTable)[] LangSysRecords { get; init; }

    public static ScriptTableRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var default_lang_sys_offset = stream.ReadOffset16();
        var lang_sys_count = stream.ReadUShortByBigEndian();
        var lang_sys_records = Lists.Repeat(() => (LangSysTag: Encoding.ASCII.GetString(stream.ReadExactly(4)), LangSysOffset: stream.ReadOffset16())).Take(lang_sys_count).ToArray();

        return new()
        {
            DefaultLangSysOffset = default_lang_sys_offset,
            LangSysCount = lang_sys_count,
            LangSysRecords = [.. lang_sys_records.Select(x => (x.LangSysTag, x.LangSysOffset, LanguageSystemTableRecord.ReadFrom(stream.SeekTo(position + x.LangSysOffset.Value))))],
        };
    }

    public int SizeOf() => DefaultLangSysOffset.SizeOf() + LangSysCount.SizeOf() + ((/* sizeof(LangSysTag) */4 + Offset16.SizeOf()) * LangSysRecords.Length);
}
