using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class DetailSection : ISection, ISubSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewModes.Detail; init => _ = value == ViewModes.Detail ? value : throw new ArgumentException(); }
    public List<ISectionElement> Elements { get; init; } = new();
}
