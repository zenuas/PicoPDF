using Mina.Extension;
using Pdf.Extension;
using System.IO;
using System.Numerics;

namespace Pdf.Operation;

public class DrawTransformationMatrix : IPathOperation
{
    public required Matrix3x2 Transform { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        var m = Transform * Matrix3x2.CreateScale(1, -1) * Matrix3x2.CreateTranslation(0, height);
        writer.Write($"{m.M11.ToPointString(option.PointFormat)} {m.M12.ToPointString(option.PointFormat)} {m.M21.ToPointString(option.PointFormat)} {m.M22.ToPointString(option.PointFormat)} {m.M31.ToPointString(option.PointFormat)} {m.M32.ToPointString(option.PointFormat)} cm\n");
    }
}
