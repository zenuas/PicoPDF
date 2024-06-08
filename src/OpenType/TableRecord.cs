using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.OpenType;

public class TableRecord(Stream stream)
{
    public readonly string TableTag = Encoding.ASCII.GetString(stream.ReadBytes(4));
    public readonly uint CheckSum = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint Offset = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));
    public readonly uint Length = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4));

    public override string ToString() => $"{TableTag}, Offset={Offset}, Length={Length}";
}
