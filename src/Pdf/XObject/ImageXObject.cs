using System.IO;

namespace PicoPDF.Pdf.XObject;

public class ImageXObject : PdfObject
{
    public required string Name { get; init; }
    public required string Path { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Image");
        _ = Elements.TryAdd("Width", Width);
        _ = Elements.TryAdd("Height", Height);
        _ = Elements.TryAdd("ColorSpace", "/DeviceRGB");
        _ = Elements.TryAdd("BitsPerComponent", 8);

        var datas = File.ReadAllBytes(Path);
        var writer = GetWriteStream(option.JpegStreamDeflate);
        writer.Write(datas);
        writer.Flush();

        Elements["Filter"] = option.JpegStreamDeflate ? "[ /FlateDecode /DCTDecode ]" : "/DCTDecode";
    }
}
