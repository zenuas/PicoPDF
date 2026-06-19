using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using Pdf.XObject.Image;
using System.IO;

namespace Pdf.Operation;

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
        writer.Write($"{imagewidth.ToPointString(option.PointFormat)} 0 0 {imageheight.ToPointString(option.PointFormat)} {(X, Y).ToPointString(height - imageheight, option.PointFormat)} cm\n");
        writer.Write($"/{Image.Name} Do\n");
        writer.Write("Q\n");
    }

    public static DrawImage Create(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => Create(
        new PointValue(x),
        new PointValue(y),
        image,
        zoomwidth,
        zoomheight
    );

    public static DrawImage Create(IPoint x, IPoint y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => new()
    {
        X = x,
        Y = y,
        Image = image,
        ZoomWidth = zoomwidth,
        ZoomHeight = zoomheight,
    };
}
