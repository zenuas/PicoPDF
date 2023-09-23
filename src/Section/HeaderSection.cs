using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class HeaderSection : ISection, IHeaderSection, IHeaderFooter
{
    public ViewModes ViewMode_ = ViewModes.First;

    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value == ViewModes.First || value == ViewModes.Every || value == ViewModes.PageFirst ? value : throw new ArgumentException(); }
    public List<ISectionElement> Elements { get; init; } = new();
    public bool DetailInclude { get => true; init => _ = value ? value : throw new ArgumentException(); }
}
