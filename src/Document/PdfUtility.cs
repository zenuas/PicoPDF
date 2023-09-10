using System;

namespace PicoPDF.Document;

public static class PdfUtility
{
    public static (int Width, int Height) GetVerticalPageSize(PageSize size)
    {
        return size switch
        {
            PageSize.A0 => (2384, 3370),
            PageSize.A1 => (1684, 2384),
            PageSize.A2 => (1191, 1684),
            PageSize.A3 => (842, 1191),
            PageSize.A4 => (595, 842),
            PageSize.A5 => (420, 595),

            PageSize.B0 => (2835, 4008),
            PageSize.B1 => (2004, 2835),
            PageSize.B2 => (1417, 2004),
            PageSize.B3 => (1001, 1417),
            PageSize.B4 => (709, 1001),
            PageSize.B5 => (499, 709),

            _ => (0, 0),
        };
    }

    public static (int Width, int Height) GetPageSize(PageSize size, Orientation orientation)
    {
        var (width, height) = GetVerticalPageSize(size);
        return orientation == Orientation.Vertical ? (width, height) : (height, width);
    }

    public static double CentimeterToPoint(double v) => v * 72 / 2.54;
    public static double MillimeterToPoint(double v) => v * 72 / 25.4;
    [Obsolete("use SI")] public static double InchToPoint(double v) => v * 72;
    public static double PointToCentimeter(double v) => v / 72 * 2.54;
    public static double PointToMillimeter(double v) => v / 72 * 25.4;
    [Obsolete("use SI")] public static double PointToInch(double v) => v / 72;
}
