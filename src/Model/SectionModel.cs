using PicoPDF.Model.Element;
using System.Collections.Generic;

namespace PicoPDF.Model;

public class SectionModel
{
    public required string Name { get; init; }
    public required int Top { get; init; }
    public required int Height { get; init; }
    public List<IModelElement> Elements { get; init; } = new();

    public override string ToString() => $"{Name}, Top={Top}, Height={Height}, Elements={Elements.Count}";
}
