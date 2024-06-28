using Mina.Extension;
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
        Version = stream.ReadUIntByBigEndian(),
        ItalicAngle = stream.ReadIntByBigEndian(),
        UnderlinePosition = stream.ReadShortByBigEndian(),
        UnderlineThickness = stream.ReadShortByBigEndian(),
        IsFixedPitch = stream.ReadUIntByBigEndian(),
        MinMemType42 = stream.ReadUIntByBigEndian(),
        MaxMemType42 = stream.ReadUIntByBigEndian(),
        MinMemType1 = stream.ReadUIntByBigEndian(),
        MaxMemType1 = stream.ReadUIntByBigEndian(),
    };
}
