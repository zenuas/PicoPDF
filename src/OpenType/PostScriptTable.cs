using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class PostScriptTable
{
    public required uint Version { get; init; }
    public required int ItalicAngle { get; init; }
    public required short UnderlinePosition { get; init; }
    public required short UnderlineThickness { get; init; }
    public required uint IsFixedPitch { get; init; }
    public required uint MinMemType42 { get; init; }
    public required uint MaxMemType42 { get; init; }
    public required uint MinMemType1 { get; init; }
    public required uint MaxMemType1 { get; init; }

    public static PostScriptTable ReadFrom(Stream stream) => new()
    {
        Version = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        ItalicAngle = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4)),
        UnderlinePosition = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        UnderlineThickness = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
        IsFixedPitch = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        MinMemType42 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        MaxMemType42 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        MinMemType1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
        MaxMemType1 = BinaryPrimitives.ReadUInt32BigEndian(stream.ReadBytes(4)),
    };
}
