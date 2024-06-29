namespace PicoPDF.OpenType;

public interface ICMapFormat : IExportable
{
    public ushort Format { get; init; }
}
