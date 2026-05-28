using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Extension;
using PicoPDF.Pdf.XObject.Image;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawImage : IOperation
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IImageXObject Image { get; init; }
    public double ZoomWidth { get; init; } = 1.0;
    public double ZoomHeight { get; init; } = 1.0;

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        var imagewidth = Image.Width * ZoomWidth;
        var imageheight = Image.Height * ZoomHeight;

        writer.Write("q\n");
        writer.Write($"{imagewidth.ToPointString(option.PointFormat)} 0 0 {imageheight.ToPointString(option.PointFormat)} {X.ToPointString(option.PointFormat)} {(height - imageheight - Y.ToPoint()).ToPointString(option.PointFormat)} cm\n");
        writer.Write($"/{Image.Name} Do\n");
        writer.Write("Q\n");
    }
}
