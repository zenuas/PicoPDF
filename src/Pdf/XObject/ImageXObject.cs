using System.IO;

namespace PicoPDF.Pdf.XObject;

public class ImageXObject : PdfObject
{
    public required string Name { get; init; }
    public required string Path { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Image");
        _ = Elements.TryAdd("Width", 300);
        _ = Elements.TryAdd("Height", 150);
        _ = Elements.TryAdd("ColorSpace", "/DeviceRGB");
        _ = Elements.TryAdd("BitsPerComponent", 8);
        _ = Elements.TryAdd("Filter", "/DCTDecode");

        var datas = File.ReadAllBytes(Path);
        Stream = new MemoryStream();
        Stream.Write(datas);
    }
}
