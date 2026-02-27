using PicoPDF.Binder.Element;
using System;

namespace PicoPDF.Binder.Data;

public class TotalSection : IFooterSection
{
    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => field; init => field = value is ViewModes.Last or ViewModes.Every ? value : throw new ArgumentException(); } = ViewModes.Last;
    public IElement[] Elements { get; init; } = [];
    public required bool PageBreak { get; init; }
    public bool IsFooter { get; init; } = false;
}
