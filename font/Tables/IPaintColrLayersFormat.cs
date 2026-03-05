namespace OpenType.Tables;

public interface IPaintColrLayersFormat : IExportable
{
    public byte Format { get; init; }

    public int SizeOf();
}
