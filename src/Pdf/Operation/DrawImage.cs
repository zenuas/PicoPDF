using Extensions;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.XObject;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawImage : IOperation
{
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required ImageXObject Image { get; init; }
    public double ZoomWidth { get; init; } = 1.0;
    public double ZoomHeight { get; init; } = 1.0;

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        var imagewidth = Image.Width * ZoomWidth;
        var imageheight = Image.Height * ZoomHeight;

        writer.Write($"q\n");
        writer.Write($"{imagewidth} 0 0 {imageheight} {X.ToPoint()} {height - imageheight - Y.ToPoint()} cm\n");
        writer.Write($"/{Image.Name} Do\n");
        writer.Write($"Q\n");
    }
}