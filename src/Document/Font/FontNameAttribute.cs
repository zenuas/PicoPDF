using System;

namespace PicoPDF.Document.Font;

public class FontNameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}
