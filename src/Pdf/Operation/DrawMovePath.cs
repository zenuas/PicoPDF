using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawMovePath : IPathOperation
{
    public required (IPoint X, IPoint Y) Start { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{PointToString(Start, height)} m\n");
    }

    public static string PointToString((IPoint X, IPoint Y) point, int height) => $"{point.X.ToPoint()} {height - point.Y.ToPoint()}";
}