namespace OpenType.Tables.GlyphSubstitution;

public interface ICoverageFormat
{
    public int? FindOrNull(uint gid);
}
