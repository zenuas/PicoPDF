using PicoPDF.Model.Element;
using PicoPDF.Section;
using System.Collections.Generic;

namespace PicoPDF.Model;

public class SectionModel
{
    public required ISection Section { get; init; }
    public int Top { get; set; }
    public List<IModelElement> Elements { get; init; } = new();

    public override string ToString() => $"{Section.Name}, Top={Top}, Height={Section.Height}, Elements={Elements.Count}";
}
