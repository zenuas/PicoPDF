using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Text;

namespace OpenType.Tables.Common;

public class ScriptRecord
{
    public required string ScriptTag { get; init; }
    public required Offset16 ScriptOffset { get; init; }
    public required ScriptTable ScriptTable { get; init; }

    public static ScriptRecord ReadFrom(Stream stream, long script_list_offset)
    {
        var script_tag = Encoding.ASCII.GetString(stream.ReadExactly(4));
        var script_offset = stream.ReadOffset16();

        return new()
        {
            ScriptTag = script_tag,
            ScriptOffset = script_offset,
            // Offset to Script table, from beginning of ScriptList
            ScriptTable = ScriptTable.ReadFrom(stream.SeekTo(script_list_offset + script_offset.Value)),
        };
    }

    public static int SizeOf() => /* sizeof(ScriptTag) */4 + /* sizeof(ScriptOffset) */Offset16.SizeOf();
}
