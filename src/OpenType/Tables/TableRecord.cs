using Mina.Extension;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType.Tables;

public class TableRecord
{
    public required string TableTag { get; init; }
    public required uint CheckSum { get; init; }
    public required uint Offset { get; init; }
    public required uint Length { get; init; }

    public static TableRecord ReadFrom(Stream stream) => new()
    {
        TableTag = Encoding.ASCII.GetString(stream.ReadExactly(4)),
        CheckSum = stream.ReadUIntByBigEndian(),
        Offset = stream.ReadUIntByBigEndian(),
        Length = stream.ReadUIntByBigEndian(),
    };

    public override string ToString() => $"{TableTag}, Offset={Offset}, Length={Length}";
}
