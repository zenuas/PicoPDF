using Binder;
using Mina.Extension;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Drawing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace PicoPDF.Pdf;

public static class PdfUtility
{
    public static readonly char[][] EscapeChars = [['('], [')'], ['\\']];

    public static string ToEscapeString(string s, Encoding encoding) => s.All(char.IsAscii) ? $"({s.ReplaceBeforeInsert(EscapeChars, ['\\']).ToStringByChars()})" : ToHexString(s, encoding);

    public static string ToHexString(string s, Encoding encoding) => $"<{Convert.ToHexStringLower(encoding.GetBytes(s))}>";

    public static DeviceRGB ToDeviceRGB(this Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document CreateDocument<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<T, PageModel, SectionModel>(section, datas, mapper), option ?? new());
    public static Document CreateDocument(string json, DataTable table, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, table), option ?? new());
    public static Document CreateDocument(string json, DataView view, PdfEventOption? option = null) => CreateDocument(json, (section) => SectionBinder.Bind<PageModel, SectionModel>(section, view), option ?? new());

    public static Document CreateDocument(string json, Func<PageSection, PageModel[]> pages, PdfEventOption option)
    {
        var opt = option ?? new();
        var document = new Document { FontRegister = opt.CreateFontRegister() };
        ModelMapping.Mapping(document, pages(JsonLoader.CreatePageFromJsonFile(json, opt)), opt);
        return document;
    }

    public static string PointToString((IPoint X, IPoint Y) point, int height, string format) => $"{PointToString(point.X.ToPoint(), format)} {PointToString(height - point.Y.ToPoint(), format)}";

    public static string PointToString(double point, string format) =>
        format == "F%" ? point.ToString("F7", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') :
            point <= long.MaxValue &&
            point >= long.MinValue &&
            point % 1d == 0d ? ((long)point).ToString() : point.ToString(format, CultureInfo.InvariantCulture);
}
