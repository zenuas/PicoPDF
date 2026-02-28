namespace OpenType.Tables.PostScript;

public class Charsets
{
    public required byte Format { get; init; }
    public required ushort[] Glyph { get; init; }
}
