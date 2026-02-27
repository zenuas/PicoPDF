using Binder.Data;
using System;

namespace PicoPDF.Binder.Data;

public class HeaderSection : IHeaderSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => field; init => field = value is ViewModes.First or ViewModes.Every or ViewModes.PageFirst ? value : throw new ArgumentException(); } = ViewModes.First;
    public IElement[] Elements { get; init; } = [];
}
