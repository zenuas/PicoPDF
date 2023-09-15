using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class TotalSection : ISection, IFooterSection
{
    public ViewModes ViewMode_ = ViewModes.Footer | ViewModes.Last;

    public required int Height { get; set; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value == (ViewModes.Total | ViewModes.Last) || value == (ViewModes.Total | ViewModes.Every) ? value : throw new ArgumentException(); }
    public List<IElement> Elements { get; init; } = new();
}
