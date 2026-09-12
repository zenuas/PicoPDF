using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Text;

namespace OpenType.Tables.Common;

public class LanguageSystemRecord
{
    public required string LangSysTag { get; init; }
    public required Offset16 LangSysOffset { get; init; }
    public required LanguageSystemTable LanguageSystemTable { get; init; }

    public static LanguageSystemRecord ReadFrom(Stream stream, long script_table_offset)
    {
        var lang_sys_tag = Encoding.ASCII.GetString(stream.ReadExactly(4));
        var lang_sys_offset = stream.ReadOffset16();

        return new()
        {
            LangSysTag = lang_sys_tag,
            LangSysOffset = lang_sys_offset,
            // 	Offset to LangSys table, from beginning of Script table.
            LanguageSystemTable = LanguageSystemTable.ReadFrom(stream.SeekTo(script_table_offset + lang_sys_offset.Value)),
        };
    }

    public static int SizeOf() => /* sizeof(LangSysTag) */4 + /* sizeof(LangSysOffset) */Offset16.SizeOf();
}
