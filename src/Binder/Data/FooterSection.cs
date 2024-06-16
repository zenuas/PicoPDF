using PicoPDF.Binder.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Binder.Data;

public class FooterSection : IFooterSection
{
    public ViewModes ViewMode_ = ViewModes.Last;

    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value is ViewModes.Last or ViewModes.Every ? value : throw new ArgumentException(); }
    public List<IElement> Elements { get; init; } = [];
    public required bool PageBreak { get; init; }
}
