using Mina.Extension;
using PicoPDF.Binder.Data;
using PicoPDF.Model.Element;
using System.Linq;

namespace PicoPDF.Model;

public class SectionModel : ISectionModel
{
    public required ISection Section { get; init; }
    public required int Depth { get; init; }
    public int Top { get; set; }
    public required int Left { get; init; }
    public int Height { get => field = Section.Height; init; }
    public bool IsFooter { get => field = Section is FooterSection; init; }
    public IModelElement[] Elements { get; init; } = [];

    public void UpdatePosition() => Elements
        .OfType<ICrossSectionModel>()
        .Where(x => x.TargetSection is { })
        .Each(x => x.UpdatePosition(this));

    public override string ToString() => $"{Section.Name}, Top={Top}, Height={Section.Height}, Elements={Elements.Length}";
}
