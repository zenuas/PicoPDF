using System.Drawing;

namespace PicoPDF.Pdf.XObject.Image;

public class ImageXObject : PdfObject, IImageXObject
{
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required Color[][] Canvas { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Image");
        _ = Elements.TryAdd("Width", Width);
        _ = Elements.TryAdd("Height", Height);
        _ = Elements.TryAdd("ColorSpace", "/DeviceRGB");
        _ = Elements.TryAdd("BitsPerComponent", 8);

        var writer = GetWriteStream(option.ImageStreamDeflate);
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var rgb = Canvas[y][x];
                writer.WriteByte(rgb.R);
                writer.WriteByte(rgb.G);
                writer.WriteByte(rgb.B);
            }
        }
        writer.Flush();
    }
}
