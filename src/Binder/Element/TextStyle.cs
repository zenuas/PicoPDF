using System;

namespace PicoPDF.Binder.Element;

[Flags]
public enum TextStyle
{
    None = 0,
    UnderLine = 1,
    UnderLine2 = 2,
    BorderTop = 4,
    BorderLeft = 8,
    BorderRight = 16,
    BorderBottom = 32,
    LineThrough = 64,
    ShrinkToFit = 128,
    Clipping = 256,
}
