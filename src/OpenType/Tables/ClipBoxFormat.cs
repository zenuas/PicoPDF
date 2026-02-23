using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class ClipBoxFormat
{
    public required byte Format { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required uint VarIndexBase { get; init; }

    public static ClipBoxFormat ReadFrom(Stream stream)
    {
        var format = stream.ReadUByte();

        return new()
        {
            Format = format,
            XMin = stream.ReadShortByBigEndian(),
            YMin = stream.ReadShortByBigEndian(),
            XMax = stream.ReadShortByBigEndian(),
            YMax = stream.ReadShortByBigEndian(),
            VarIndexBase = format >= 2 ? stream.ReadUIntByBigEndian() : 0,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteShortByBigEndian(XMin);
        stream.WriteShortByBigEndian(YMin);
        stream.WriteShortByBigEndian(XMax);
        stream.WriteShortByBigEndian(YMax);

        if (Format >= 2)
        {
            stream.WriteUIntByBigEndian(VarIndexBase);
        }
    }
}
