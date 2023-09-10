using PicoPDF.Section.Element;
using System.Collections.Generic;

namespace PicoPDF.Section;

public interface ISection
{
    public int Height { get; set; }
    public ViewModes ViewMode { get; init; }
    public List<IElement> Elements { get; init; }
}
