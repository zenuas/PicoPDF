using System;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class StandardBitmapGraphicsTable : IExportable
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
