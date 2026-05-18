using Mina.Extension;
using System.IO;
using System.Numerics;

namespace PicoPDF.Pdf.Operation;

public class DrawTransformationMatrix : IPathOperation
{
    public required Matrix3x2 Transform { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        var m = Transform * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(0, height);
        writer.Write($"{PdfUtility.PointToString(m.M11, option.PointFormat)} {PdfUtility.PointToString(m.M12, option.PointFormat)} {PdfUtility.PointToString(m.M21, option.PointFormat)} {PdfUtility.PointToString(m.M22, option.PointFormat)} {PdfUtility.PointToString(m.M31, option.PointFormat)} {PdfUtility.PointToString(m.M32, option.PointFormat)} cm\n");
    }
}
