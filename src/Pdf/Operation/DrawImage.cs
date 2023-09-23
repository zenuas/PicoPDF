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

    public void OperationWrite(int width, int height, Stream writer)
    {
        writer.Write($"q\n");
        writer.Write($"1 0 0 1 {X.ToPoint()} {Y.ToPoint()} cm\n");
        writer.Write($"/{Image.Name} Do\n");
        writer.Write($"Q\n");
    }
}