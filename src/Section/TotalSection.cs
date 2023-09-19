using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class TotalSection : ISection, IFooterSection, IHeaderFooter
{
    public ViewModes ViewMode_ = ViewModes.Footer | ViewModes.Last;

    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value == (ViewModes.Total | ViewModes.Last) || value == (ViewModes.Total | ViewModes.Every) ? value : throw new ArgumentException(); }
    public List<ISectionElement> Elements { get; init; } = new();
    public required bool PageBreak { get; init; }
    public bool DetailInclude { get; init; } = true;
}
