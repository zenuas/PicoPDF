using System.IO;

namespace PicoPDF.OpenType.Tables;

public class ColorRecord
{
    public required byte Blue { get; init; }
    public required byte Green { get; init; }
    public required byte Red { get; init; }
    public required byte Alpha { get; init; }

    public static ColorRecord ReadFrom(Stream stream) => new()
    {
        Blue = (byte)stream.ReadByte(),
        Green = (byte)stream.ReadByte(),
        Red = (byte)stream.ReadByte(),
        Alpha = (byte)stream.ReadByte(),
    };
}
