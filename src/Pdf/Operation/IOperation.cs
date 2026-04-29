using PicoPDF.Pdf.Drawing;
using System.Globalization;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public interface IOperation
{
    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option);

    public static string PointToString((IPoint X, IPoint Y) point, int height, string format) => $"{PointToString(point.X.ToPoint(), format)} {PointToString(height - point.Y.ToPoint(), format)}";
    public static string PointToString(double point, string format) =>
        point <= long.MaxValue &&
        point >= long.MinValue &&
        point % 1d == 0d ? ((long)point).ToString() : point.ToString(format, CultureInfo.InvariantCulture);
}
