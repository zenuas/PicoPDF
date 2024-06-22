﻿using System.Collections.Generic;

namespace PicoPDF.OpenType;

public interface IOpenTypeHeader
{
    public string FontFamily { get; init; }
    public string Style { get; init; }
    public string FullFontName { get; init; }
    public string PostScriptName { get; init; }
    public IFontPath Path { get; init; }
    public long Position { get; init; }
    public Dictionary<string, TableRecord> TableRecords { get; init; }
    public OffsetTable Offset { get; init; }
    public (string Name, NameRecord NameRecord)[] NameRecords { get; init; }
}