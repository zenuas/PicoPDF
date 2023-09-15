using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class HeaderSection : ISection, IHeaderSection
{
    public ViewModes ViewMode_ = ViewModes.Header | ViewModes.First;

    public required int Height { get; set; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value == (ViewModes.Header | ViewModes.First) || value == (ViewModes.Header | ViewModes.Every) ? value : throw new ArgumentException(); }
    public List<IElement> Elements { get; init; } = new();
}
