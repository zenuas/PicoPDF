using System;
using System.IO;

namespace OpenType.Tables;

public record class StandardBitmapGraphicsTable : IExportable
{
    public static StandardBitmapGraphicsTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
