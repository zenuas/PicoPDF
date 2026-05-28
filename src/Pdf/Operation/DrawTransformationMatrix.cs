using Mina.Extension;
using PicoPDF.Pdf.Extension;
using System.IO;
using System.Numerics;

namespace PicoPDF.Pdf.Operation;

public class DrawTransformationMatrix : IPathOperation
{
    public required Matrix3x2 Transform { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        var m = Transform * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(0, height);
        writer.Write($"{m.M11.PointToString(option.PointFormat)} {m.M12.PointToString(option.PointFormat)} {m.M21.PointToString(option.PointFormat)} {m.M22.PointToString(option.PointFormat)} {m.M31.PointToString(option.PointFormat)} {m.M32.PointToString(option.PointFormat)} cm\n");
    }
}
