using PicoPDF.Binder.Element;
using System.Collections.Generic;

namespace PicoPDF.Binder.Data;

public interface ISection
{
    public string Name { get; init; }
    public int Height { get; init; }
    public ViewModes ViewMode { get; init; }
    public List<ISectionElement> Elements { get; init; }
}
