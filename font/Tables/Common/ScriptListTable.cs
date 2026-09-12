using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType.Tables.Common;

public class ScriptListTable
{
    public required ushort ScriptCount { get; init; }
    public required (string ScriptTag, Offset16 ScriptOffset, ScriptTable ScriptTable)[] ScriptRecords { get; init; }

    public static ScriptListTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var script_count = stream.ReadUShortByBigEndian();
        var script_records = Lists.Repeat(() => (ScriptTag: Encoding.ASCII.GetString(stream.ReadExactly(4)), ScriptOffset: stream.ReadOffset16())).Take(script_count).ToArray();

        return new()
        {
            ScriptCount = script_count,
            ScriptRecords = [.. script_records.Select(x => (x.ScriptTag, x.ScriptOffset, ScriptTable.ReadFrom(stream.SeekTo(position + x.ScriptOffset.Value))))],
        };
    }

    public int SizeOf() => ScriptCount.SizeOf() + ((/* sizeof(ScriptTag) */4 + Offset16.SizeOf()) * ScriptRecords.Length);
}
