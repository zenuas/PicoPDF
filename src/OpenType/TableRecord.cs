using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType;

public class TableRecord
{
    public required string TableTag { get; init; }
    public required uint CheckSum { get; init; }
    public required uint Offset { get; init; }
    public required uint Length { get; init; }

    public static TableRecord ReadFrom(Stream stream) => new()
    {
        TableTag = Encoding.ASCII.GetString(stream.ReadBytes(4)),
        CheckSum = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        Offset = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        Length = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
    };

    public override string ToString() => $"{TableTag}, Offset={Offset}, Length={Length}";
}
