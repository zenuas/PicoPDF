namespace OpenType.Tables;

public interface IPaintFormat : IExportable
{
    public byte Format { get; init; }

    public int SizeOf();
}
