using System.IO;

namespace OpenType.Tables;

public record class ColorBitmapDataTable
{
    public static ColorBitmapDataTable ReadFrom(Stream stream)
    {
        return new();
    }
}
