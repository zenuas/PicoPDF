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
using System.Text;

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

    public static readonly byte[] EscapeBytes = Encoding.ASCII.GetBytes("()\\");

    public static string ToStringEscapeBytes(string s) => s.All(char.IsAscii)
        ? Encoding.ASCII.GetString([.. ToStringEscapeBytes(s, Encoding.ASCII)])
        : $"<{Convert.ToHexStringLower([.. Encoding.BigEndianUnicode.GetPreamble(), .. Encoding.BigEndianUnicode.GetBytes(s)])}>";
    public static IEnumerable<byte> ToStringEscapeBytes(string s, Encoding encoding) => ToStringEscapeBytes(encoding.GetBytes(s));
    public static IEnumerable<byte> ToStringEscapeBytes(byte[] bytes) => [(byte)'(', .. bytes.Select<byte, byte[]>(x => x.In(EscapeBytes) ? [(byte)'\\', x] : [x]).Flatten(), (byte)')'];

    public static DeviceRGB ToDeviceRGB(this System.Drawing.Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document Create<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => Create(JsonLoader.Load(json), datas, mapper, register);
    public static Document Create(string json, DataTable table, IFontRegister? register = null) => Create(JsonLoader.Load(json), table, register);
    public static Document Create(string json, DataView view, IFontRegister? register = null) => Create(JsonLoader.Load(json), view, register);
    public static Document Create<T>(PageSection pagesection, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, datas, mapper)));
    public static Document Create(PageSection pagesection, DataTable table, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, table)));
    public static Document Create(PageSection pagesection, DataView view, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, view)));

    public static FontRegister CreateDefaultFontRegister() => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetSystemFontDirectories()]));
}
