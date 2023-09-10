﻿using PicoPDF.Section.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Section;

public class DetailSection : ISection, ISubSection
{
    public required int Height { get; set; }
    public ViewModes ViewMode { get => ViewModes.Detail; init => _ = value == ViewModes.Detail ? value : throw new ArgumentException(); }
    public List<IElement> Elements { get; init; } = new();
}
