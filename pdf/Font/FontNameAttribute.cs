using System;

namespace Pdf.Font;

[AttributeUsage(AttributeTargets.Field)]
public class FontNameAttribute(string name) : Attribute
{
    public string Name { get; init; } = name;
}
