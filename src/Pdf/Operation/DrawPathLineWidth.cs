using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawPathLineWidth : IPathOperation
{
    public IPoint LineWidth { get; init; } = new PointValue(1);

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{PdfUtility.PointToString(LineWidth.ToPoint(), option.PointFormat)} w\n");
    }
}
