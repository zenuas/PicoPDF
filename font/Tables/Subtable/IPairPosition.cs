namespace OpenType.Tables.Subtable;

public interface IPairPosition
{
    public (ValueRecord First, ValueRecord Second)? GetPosition(uint first_gid, uint second_gid);
}
