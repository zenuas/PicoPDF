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

    public static IEnumerable<byte> ToStringEscapeBytes(string s) => Encoding.UTF8.GetBytes(s).Length == s.Length
        ? ToStringEscapeBytes(s, Encoding.ASCII)
        : [
            (byte)'(',
            .. Encoding.BigEndianUnicode.GetPreamble(),
            .. ToEscapeBytes(Encoding.BigEndianUnicode.GetBytes(s)),
            (byte)')'
        ];
    public static IEnumerable<byte> ToStringEscapeBytes(string s, Encoding encoding) => ToStringEscapeBytes(encoding.GetBytes(s));
    public static IEnumerable<byte> ToStringEscapeBytes(byte[] bytes) => [(byte)'(', .. ToEscapeBytes(bytes), (byte)')'];
    public static IEnumerable<byte> ToEscapeBytes(byte[] bytes) => [.. bytes.Select<byte, byte[]>(x => x.In(EscapeBytes) ? [(byte)'\\', x] : [x]).Flatten()];

    public static DeviceRGB ToDeviceRGB(this System.Drawing.Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document Create<T>(FontRegister register, string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null) => Create(register, JsonLoader.Load(json), datas, mapper);
    public static Document Create(FontRegister register, string json, DataTable table) => Create(register, JsonLoader.Load(json), table);
    public static Document Create(FontRegister register, string json, DataView view) => Create(register, JsonLoader.Load(json), view);
    public static Document Create<T>(FontRegister register, PageSection pagesection, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null) => new Document { FontRegister = register }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, datas, mapper)));
    public static Document Create(FontRegister register, PageSection pagesection, DataTable table) => new Document { FontRegister = register }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, table)));
    public static Document Create(FontRegister register, PageSection pagesection, DataView view) => new Document { FontRegister = register }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, view)));
}
