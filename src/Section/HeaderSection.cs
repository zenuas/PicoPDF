﻿using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class HeaderSection : ISection, IHeaderSection, IHeaderFooter
{
    public ViewModes ViewMode_ = ViewModes.Header | ViewModes.First;

    public required string Name { get; init; }
    public required int Height { get; init; }
    public ViewModes ViewMode { get => ViewMode_; init => ViewMode_ = value == (ViewModes.Header | ViewModes.First) || value == (ViewModes.Header | ViewModes.Every) ? value : throw new ArgumentException(); }
    public List<ISectionElement> Elements { get; init; } = new();
    public bool DetailInclude { get; init; } = true;
}
