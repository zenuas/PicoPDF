using Mina.Extension;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace PicoPDF.TrueType;

public struct TableRecord
{
    public string TableTag;
    public uint CheckSum;
    public uint Offset;
    public uint Length;

    public static TableRecord ReadFrom(Stream stream) => new()
    {
        TableTag = Encoding.ASCII.GetString(stream.ReadBytes(4)),
        CheckSum = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        Offset = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        Length = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4))
    };

    public override string ToString() => $"{TableTag}, Offset={Offset}, Length={Length}";
}
