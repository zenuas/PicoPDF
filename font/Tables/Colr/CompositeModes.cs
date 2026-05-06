namespace OpenType.Tables.Colr;

public enum CompositeModes : byte
{
    // Porter-Duff modes

    /// <summary>
    /// Clear
    /// </summary>
    COMPOSITE_CLEAR = 0,

    /// <summary>
    /// Source ("Copy" in Composition & Blending Level 1)
    /// </summary>
    COMPOSITE_SRC = 1,

    /// <summary>
    /// Destination
    /// </summary>
    COMPOSITE_DEST = 2,

    /// <summary>
    /// Source Over
    /// </summary>
    COMPOSITE_SRC_OVER = 3,

    /// <summary>
    /// Destination Over
    /// </summary>
    COMPOSITE_DEST_OVER = 4,

    /// <summary>
    /// Source In
    /// </summary>
    COMPOSITE_SRC_IN = 5,

    /// <summary>
    /// Destination In
    /// </summary>
    COMPOSITE_DEST_IN = 6,

    /// <summary>
    /// Source Out
    /// </summary>
    COMPOSITE_SRC_OUT = 7,

    /// <summary>
    /// Destination Out
    /// </summary>
    COMPOSITE_DEST_OUT = 8,

    /// <summary>
    /// Source Atop
    /// </summary>
    COMPOSITE_SRC_ATOP = 9,

    /// <summary>
    /// Destination Atop
    /// </summary>
    COMPOSITE_DEST_ATOP = 10,

    /// <summary>
    /// XOR
    /// </summary>
    COMPOSITE_XOR = 11,

    /// <summary>
    /// Plus ("Lighter" in Composition & Blending Level 1)
    /// </summary>
    COMPOSITE_PLUS = 12,

    // Separable color blend modes:

    /// <summary>
    /// screen
    /// </summary>
    COMPOSITE_SCREEN = 13,

    /// <summary>
    /// overlay
    /// </summary>
    COMPOSITE_OVERLAY = 14,

    /// <summary>
    /// darken
    /// </summary>
    COMPOSITE_DARKEN = 15,

    /// <summary>
    /// lighten
    /// </summary>
    COMPOSITE_LIGHTEN = 16,

    /// <summary>
    /// color-dodge
    /// </summary>
    COMPOSITE_COLOR_DODGE = 17,

    /// <summary>
    /// color-burn
    /// </summary>
    COMPOSITE_COLOR_BURN = 18,

    /// <summary>
    /// hard-light
    /// </summary>
    COMPOSITE_HARD_LIGHT = 19,

    /// <summary>
    /// soft-light
    /// </summary>
    COMPOSITE_SOFT_LIGHT = 20,

    /// <summary>
    /// difference
    /// </summary>
    COMPOSITE_DIFFERENCE = 21,

    /// <summary>
    /// exclusion
    /// </summary>
    COMPOSITE_EXCLUSION = 22,

    /// <summary>
    /// multiply
    /// </summary>
    COMPOSITE_MULTIPLY = 23,

    // Non-separable color blend modes:

    /// <summary>
    /// hue
    /// </summary>
    COMPOSITE_HSL_HUE = 24,

    /// <summary>
    /// saturation
    /// </summary>
    COMPOSITE_HSL_SATURATION = 25,

    /// <summary>
    /// color
    /// </summary>
    COMPOSITE_HSL_COLOR = 26,

    /// <summary>
    /// luminosity
    /// </summary>
    COMPOSITE_HSL_LUMINOSITY = 27,

}
