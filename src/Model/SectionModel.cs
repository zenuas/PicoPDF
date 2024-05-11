using PicoPDF.Binder.Data;
using PicoPDF.Model.Element;
using System.Collections.Generic;

namespace PicoPDF.Model;

public class SectionModel
{
    public required ISection Section { get; init; }
    public int Top { get; set; }
    public List<IModelElement> Elements { get; init; } = [];

    public override string ToString() => $"{Section.Name}, Top={Top}, Height={Section.Height}, Elements={Elements.Count}";
}
