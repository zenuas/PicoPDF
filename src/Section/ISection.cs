using PicoPDF.Section.Element;
using System.Collections.Generic;

namespace PicoPDF.Section;

public interface ISection
{
    public string Name { get; init; }
    public int Height { get; init; }
    public ViewModes ViewMode { get; init; }
    public List<IElement> Elements { get; init; }
}
