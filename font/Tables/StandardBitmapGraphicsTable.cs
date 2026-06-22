using System.IO;

namespace OpenType.Tables;

public record class StandardBitmapGraphicsTable
{
    public static StandardBitmapGraphicsTable ReadFrom(Stream stream)
    {
        return new();
    }
}
