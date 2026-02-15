using PicoPDF.Binder.Data;
using PicoPDF.Model.Element;

namespace PicoPDF.Model;

public class SectionModel
{
    public required ISection Section { get; init; }
    public required int? Depth { get; init; }
    public int Top { get; set; }
    public required int Left { get; init; }
    public IModelElement[] Elements { get; init; } = [];

    public override string ToString() => $"{Section.Name}, Top={Top}, Height={Section.Height}, Elements={Elements.Length}";
}
