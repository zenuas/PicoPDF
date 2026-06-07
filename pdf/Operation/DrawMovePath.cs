using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawMovePath : IPathOperation
{
    public required (IPoint X, IPoint Y) Start { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{Start.ToPointString(height, option.PointFormat)} m\n");
    }
}
