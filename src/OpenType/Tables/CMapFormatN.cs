﻿using System.IO;

namespace PicoPDF.OpenType.Tables;

public class CMapFormatN : ICMapFormat
{
    public required ushort Format { get; init; }

    public void WriteTo(Stream stream) { }
}
