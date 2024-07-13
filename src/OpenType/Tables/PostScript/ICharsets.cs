namespace PicoPDF.OpenType.Tables.PostScript;

public interface ICharsets
{
    public byte Format { get; init; }
    public ushort[] Glyph { get; init; }
}
