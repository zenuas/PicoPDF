using System;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class ColorTable : IExportable
{
    public static ColorTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
