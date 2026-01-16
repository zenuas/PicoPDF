using System;

namespace PicoPDF.Binder.Element;

[Flags]
public enum TextStyle
{
    None = 0,
    Underline = 1,
    DoubleUnderline = 2,
    BorderTop = 4,
    BorderLeft = 8,
    BorderRight = 16,
    BorderBottom = 32,
    Strikethrough = 64,
    ShrinkToFit = 128,
    Clipping = 256,
}
