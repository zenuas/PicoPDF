using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Model;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PicoPDF.Pdf;

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

    public static readonly char[] EscapeChars = ['(', ')', '\\'];

    public static string ToEscapeString(string s, System.Text.Encoding encoding) => s.All(char.IsAscii) ? $"({s.Select<char, char[]>(x => x.In(EscapeChars) ? ['\\', x] : [x]).Flatten().ToStringByChars()})" : ToHexString(s, encoding);

    public static string ToHexString(string s, System.Text.Encoding encoding) => $"<{Convert.ToHexStringLower(encoding.GetBytes(s))}>";

    public static DeviceRGB ToDeviceRGB(this System.Drawing.Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document CreateDocument<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), datas, mapper, register);
    public static Document CreateDocument(string json, DataTable table, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), table, register);
    public static Document CreateDocument(string json, DataView view, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), view, register);
    public static Document CreateDocument<T>(PageSection pagesection, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, datas, mapper)));
    public static Document CreateDocument(PageSection pagesection, DataTable table, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, table)));
    public static Document CreateDocument(PageSection pagesection, DataView view, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, view)));

    public static IFontRegister CreateDefaultFontRegister() => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetSystemFontDirectories()]));
}
