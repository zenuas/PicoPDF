using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenType.Tables.Common;

public class ScriptListTable
{
    public required ushort ScriptCount { get; init; }
    public required ScriptRecord[] ScriptRecords { get; init; }

    public static ScriptListTable ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var script_count = stream.ReadUShortByBigEndian();

        var script_records_offset = stream.Position;
        var script_records = Lists.Repeat(() => (ScriptTag: Encoding.ASCII.GetString(stream.ReadExactly(4)), ScriptOffset: stream.ReadOffset16())).Take(script_count).ToArray();

        return new()
        {
            ScriptCount = script_count,
            ScriptRecords = [.. Lists.Sequence(script_records_offset, ScriptRecord.SizeOf()).Select(x => ScriptRecord.ReadFrom(stream.SeekTo(x), position)).Take(script_count)],
        };
    }

    public int SizeOf() => ScriptCount.SizeOf() + (ScriptRecord.SizeOf() * ScriptRecords.Length);
}
