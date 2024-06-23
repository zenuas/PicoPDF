namespace PicoPDF.OpenType.TrueType;

public enum SimpleGlyphFlags
{
    ON_CURVE_POINT = 1 << 0,
    X_SHORT_VECTOR = 1 << 1,
    Y_SHORT_VECTOR = 1 << 2,
    REPEAT_FLAG = 1 << 3,
    X_IS_SAME_OR_POSITIVE_X_SHORT_VECTOR = 1 << 4,
    Y_IS_SAME_OR_POSITIVE_Y_SHORT_VECTOR = 1 << 5,
    OVERLAP_SIMPLE = 1 << 6,
    Reserved = 1 << 7,
}
