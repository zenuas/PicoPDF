using PicoPDF.Binder.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Binder.Data;

public class DetailSection : ISection, ISubSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewModes.Every; init => _ = value == ViewModes.Every ? value : throw new ArgumentException(); }
    public List<ISectionElement> Elements { get; init; } = new();
}
