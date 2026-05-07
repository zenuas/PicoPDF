using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawLinePath : IPathOperation
{
    public required (IPoint X, IPoint Y) End { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"{PdfUtility.PointToString(End, height, option.PointFormat)} l\n");
    }
}
