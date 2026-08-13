namespace OpenType.Tables.Subtable;

public interface ISinglePosition
{
    public ValueRecord? GetPosition(uint gid);
}
