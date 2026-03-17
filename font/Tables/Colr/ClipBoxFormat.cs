using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class ClipBoxFormat : IExportable
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
            XMin = stream.ReadFWORD(),
            YMin = stream.ReadFWORD(),
            XMax = stream.ReadFWORD(),
            YMax = stream.ReadFWORD(),
            VarIndexBase = format >= 2 ? stream.ReadUIntByBigEndian() : 0,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteFWORD(XMin);
        stream.WriteFWORD(YMin);
        stream.WriteFWORD(XMax);
        stream.WriteFWORD(YMax);

        if (Format >= 2)
        {
            stream.WriteUIntByBigEndian(VarIndexBase);
        }
    }

    public int SizeOf() => Format.SizeOf() + XMin.SizeOf() + YMin.SizeOf() + XMax.SizeOf() + YMax.SizeOf() + (Format >= 2 ? VarIndexBase.SizeOf() : 0);
}
