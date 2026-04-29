using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawMovePath : IPathOperation
{
    public required (IPoint X, IPoint Y) Start { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{IOperation.PointToString(Start, height, option.PointFormat)} m\n");
    }
}
