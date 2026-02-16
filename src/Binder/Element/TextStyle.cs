using System;

namespace PicoPDF.Binder.Element;

[Flags]
public enum TextStyle
{
    None = 0,
    Underline = 1 << 0,
    DoubleUnderline = 1 << 1,
    BorderTop = 1 << 2,
    BorderLeft = 1 << 3,
    BorderRight = 1 << 4,
    BorderBottom = 1 << 5,
    Strikethrough = 1 << 6,
    DoubleStrikethrough = 1 << 7,
    ShrinkToFit = 1 << 8,
    Clipping = 1 << 9,
}
