using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class LanguageTagRecord
{
    public required ushort Length { get; init; }
    public required ushort LanguageTagOffset { get; init; }

    public static LanguageTagRecord ReadFrom(Stream stream) => new()
    {
        Length = stream.ReadUShortByBigEndian(),
        LanguageTagOffset = stream.ReadUShortByBigEndian(),
    };
}
