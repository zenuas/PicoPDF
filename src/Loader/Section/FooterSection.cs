using Binder.Data;
using System;

namespace PicoPDF.Loader.Section;

public class FooterSection : IFooterSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => field; init => field = value is ViewModes.Last or ViewModes.Every ? value : throw new ArgumentException(); } = ViewModes.Last;
    public IElement[] Elements { get; init; } = [];
    public required bool PageBreak { get; init; }
    public bool IsFooter { get; init; } = true;
}
