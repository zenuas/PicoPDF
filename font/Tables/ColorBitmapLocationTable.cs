using System;
using System.IO;

namespace OpenType.Tables;

public class ColorBitmapLocationTable : IExportable
{
    public static ColorBitmapLocationTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
