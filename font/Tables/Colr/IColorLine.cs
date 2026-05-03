namespace OpenType.Tables.Colr;

public interface IColorLine
{
    public Extend Extend { get; init; }
    public ushort NumberOfStops { get; init; }
}
