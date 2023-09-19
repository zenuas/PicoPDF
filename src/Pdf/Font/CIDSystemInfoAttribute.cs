using System;

namespace PicoPDF.Pdf.Font;

public class CIDSystemInfoAttribute(string name, string registry, string ordering, int supplement) : Attribute
{
    public string Name { get; init; } = name;
    public string Registry { get; init; } = registry;
    public string Ordering { get; init; } = ordering;
    public int Supplement { get; init; } = supplement;
}
