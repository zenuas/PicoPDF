using OpenType.Tables;

namespace OpenType;

public interface IOpenTypeFont : IOpenTypeRequiredTables
{
    public ColorBitmapDataTable? ColorBitmapData { get; init; }
    public ColorBitmapLocationTable? ColorBitmapLocation { get; init; }
    public ColorTable? Color { get; init; }
    public ColorPaletteTable? ColorPalette { get; init; }
    public StandardBitmapGraphicsTable? StandardBitmapGraphics { get; init; }
    public ScalableVectorGraphicsTable? ScalableVectorGraphics { get; init; }
}
