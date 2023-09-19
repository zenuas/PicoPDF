using System;

namespace PicoPDF.Pdf.Font;

public class FontNameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}
