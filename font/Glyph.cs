using OpenType.Tables;
using OpenType.Tables.Subtable;
using Svg.Outline;

namespace OpenType;

public readonly record struct Glyph(
        IOutline[] Outlines,
        HorizontalMetrics HorizontalMetrics,
        (int Height, int TopSideBearing)? VerticalMetrics,
        ValueRecord? Position,
        bool NoOutline,
        bool Notdef,
        float Ascender,
        float Descender,
        float XMin,
        float XMax,
        float YMin,
        float YMax,
        int Char,
        uint OldGID
    );
