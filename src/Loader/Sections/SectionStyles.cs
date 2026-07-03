using System;

namespace PicoPDF.Loader.Sections;

[Flags]
public enum SectionStyles
{
    None = 0,
    PageBreak = 1 << 0,
    Fill = 1 << 1,
    Clipping = 1 << 2,
    HeightAdjusting = 1 << 3,
}
