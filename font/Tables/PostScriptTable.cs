using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class PostScriptTable : IExportable
{
    public required uint Version { get; init; }
    public required uint ItalicAngle { get; init; }
    public required short UnderlinePosition { get; init; }
    public required short UnderlineThickness { get; init; }
    public required uint IsFixedPitch { get; init; }
    public required uint MinMemType42 { get; init; }
    public required uint MaxMemType42 { get; init; }
    public required uint MinMemType1 { get; init; }
    public required uint MaxMemType1 { get; init; }

    public static PostScriptTable ReadFrom(Stream stream) => new()
    {
        Version = stream.ReadUIntByBigEndian(),
        ItalicAngle = stream.ReadFixed(),
        UnderlinePosition = stream.ReadFWORD(),
        UnderlineThickness = stream.ReadFWORD(),
        IsFixedPitch = stream.ReadUIntByBigEndian(),
        MinMemType42 = stream.ReadUIntByBigEndian(),
        MaxMemType42 = stream.ReadUIntByBigEndian(),
        MinMemType1 = stream.ReadUIntByBigEndian(),
        MaxMemType1 = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUIntByBigEndian(Version);
        stream.WriteFixed(ItalicAngle);
        stream.WriteFWORD(UnderlinePosition);
        stream.WriteFWORD(UnderlineThickness);
        stream.WriteUIntByBigEndian(IsFixedPitch);
        stream.WriteUIntByBigEndian(MinMemType42);
        stream.WriteUIntByBigEndian(MaxMemType42);
        stream.WriteUIntByBigEndian(MinMemType1);
        stream.WriteUIntByBigEndian(MaxMemType1);
    }
}
