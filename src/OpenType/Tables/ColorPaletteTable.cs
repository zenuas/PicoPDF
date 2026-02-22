using System;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class ColorPaletteTable : IExportable
{
    public static ColorPaletteTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
