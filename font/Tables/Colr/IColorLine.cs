namespace OpenType.Tables.Colr;

public interface IColorLine
{
    public byte Extend { get; init; }
    public ushort NumberOfStops { get; init; }
}
