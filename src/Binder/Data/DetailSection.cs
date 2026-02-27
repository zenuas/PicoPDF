using PicoPDF.Binder.Element;
using System;

namespace PicoPDF.Binder.Data;

public class DetailSection : IDetailSection, ISubSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewModes.Every; init => _ = value == ViewModes.Every ? value : throw new ArgumentException(); }
    public IElement[] Elements { get; init; } = [];
}
