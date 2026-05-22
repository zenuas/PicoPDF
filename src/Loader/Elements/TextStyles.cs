using System;

namespace PicoPDF.Loader.Elements;

[Flags]
public enum TextStyles
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
    MultiLine = 1 << 9,
    LineBreak = 1 << 10,
    Clipping = 1 << 11,
    Stroke = 1 << 12,
    Border = BorderTop | BorderLeft | BorderRight | BorderBottom,

    TextStyleMask = Underline | DoubleUnderline | Strikethrough | DoubleStrikethrough,
    BorderStyleMask = Border,
}
