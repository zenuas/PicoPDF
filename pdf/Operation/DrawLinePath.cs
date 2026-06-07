using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawLinePath : IPathOperation
{
    public required (IPoint X, IPoint Y) End { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{End.ToPointString(height, option.PointFormat)} l\n");
    }
}
