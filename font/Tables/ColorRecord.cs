using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class ColorRecord
{
    public required byte Blue { get; init; }
    public required byte Green { get; init; }
    public required byte Red { get; init; }
    public required byte Alpha { get; init; }

    public static ColorRecord ReadFrom(Stream stream) => new()
    {
        Blue = stream.ReadUByte(),
        Green = stream.ReadUByte(),
        Red = stream.ReadUByte(),
        Alpha = stream.ReadUByte(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Blue);
        stream.WriteByte(Green);
        stream.WriteByte(Red);
        stream.WriteByte(Alpha);
    }
}
