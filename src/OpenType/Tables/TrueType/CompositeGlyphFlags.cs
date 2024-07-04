namespace PicoPDF.OpenType.Tables.TrueType;

public enum CompositeGlyphFlags
{
    ARG_1_AND_2_ARE_WORDS = 1 << 0,
    ARGS_ARE_XY_VALUES = 1 << 1,
    ROUND_XY_TO_GRID = 1 << 2,
    WE_HAVE_A_SCALE = 1 << 3,
    MORE_COMPONENTS = 1 << 5,
    WE_HAVE_AN_X_AND_Y_SCALE = 1 << 6,
    WE_HAVE_A_TWO_BY_TWO = 1 << 7,
    WE_HAVE_INSTRUCTIONS = 1 << 8,
    USE_MY_METRICS = 1 << 9,
    OVERLAP_COMPOUND = 1 << 10,
    SCALED_COMPONENT_OFFSET = 1 << 11,
    UNSCALED_COMPONENT_OFFSET = 1 << 12,
    Reserved = 0xE010,
}
