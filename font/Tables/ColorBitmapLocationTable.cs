using System.IO;

namespace OpenType.Tables;

public record class ColorBitmapLocationTable
{
    public static ColorBitmapLocationTable ReadFrom(Stream stream)
    {
        return new();
    }
}
