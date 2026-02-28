using System;
using System.IO;

namespace OpenType.Tables;

public class ColorBitmapDataTable : IExportable
{
    public static ColorBitmapDataTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
