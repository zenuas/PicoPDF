using Mina.Extension;
using Pdf.Drawing;
using Pdf.Extension;
using System.IO;

namespace Pdf.Operation;

public class DrawPathLineWidth : IPathOperation
{
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{LineWidth.ToPointString(option.PointFormat)} w\n");
    }
}
