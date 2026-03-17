using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class LanguageTagRecord
{
    public required ushort Length { get; init; }
    public required ushort LanguageTagOffset { get; init; }

    public static LanguageTagRecord ReadFrom(Stream stream) => new()
    {
        Length = stream.ReadUShortByBigEndian(),
        LanguageTagOffset = stream.ReadOffset16(),
    };

    public int SizeOf() => Length.SizeOf() + LanguageTagOffset.SizeOf();
}
