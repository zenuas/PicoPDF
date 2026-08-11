namespace OpenType.Tables.Subtable;

public interface ICoverageFormat
{
    public int? FindOrNull(uint gid);
}
