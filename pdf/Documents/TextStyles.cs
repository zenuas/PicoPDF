using System;

namespace Pdf.Documents;

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
    Clipping = 1 << 10,
    Stroke = 1 << 11,
    Border = BorderTop | BorderLeft | BorderRight | BorderBottom,

    LineBreakSimplifiedChinese = 1 << 12,
    LineBreakTraditionalChinese = 1 << 13,
    LineBreakJapanese = 1 << 14,
    LineBreakKorean = 1 << 15,
    LineBreak = LineBreakSimplifiedChinese | LineBreakTraditionalChinese | LineBreakJapanese | LineBreakKorean,

    TextStyleMask = Underline | DoubleUnderline | Strikethrough | DoubleStrikethrough,
    BorderStyleMask = Border,
    BreakMask = LineBreak,
}
