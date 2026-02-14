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
    public static double CentimeterToPoint(double v) => v * 72 / 2.54;
    public static double MillimeterToPoint(double v) => v * 72 / 25.4;
    [Obsolete("use SI")] public static double InchToPoint(double v) => v * 72;
    public static double PointToCentimeter(double v) => v / 72 * 2.54;
    public static double PointToMillimeter(double v) => v / 72 * 25.4;
    [Obsolete("use SI")] public static double PointToInch(double v) => v / 72;

    public static readonly char[][] EscapeChars = [['('], [')'], ['\\']];

    public static string ToEscapeString(string s, Encoding encoding) => s.All(char.IsAscii) ? $"({s.ReplaceBeforeInsert(EscapeChars, ['\\']).ToStringByChars()})" : ToHexString(s, encoding);

    public static string ToHexString(string s, Encoding encoding) => $"<{Convert.ToHexStringLower(encoding.GetBytes(s))}>";

    public static DeviceRGB ToDeviceRGB(this System.Drawing.Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document CreateDocument<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), datas, mapper, register);
    public static Document CreateDocument(string json, DataTable table, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), table, register);
    public static Document CreateDocument(string json, DataView view, IFontRegister? register = null) => CreateDocument(JsonLoader.Load(json), view, register);
    public static Document CreateDocument<T>(PageSection pagesection, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, datas, mapper)));
    public static Document CreateDocument(PageSection pagesection, DataTable table, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, table)));
    public static Document CreateDocument(PageSection pagesection, DataView view, IFontRegister? register = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind(pagesection, view)));

    public static IFontRegister CreateDefaultFontRegister() => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetSystemFontDirectories()]));

    public static IEnumerable<int> ToUtf32CharArray(string s)
    {
        var i = 0;
        for (; i < s.Length - 1; i++)
        {
            yield return char.IsHighSurrogate(s[i]) && char.IsLowSurrogate(s[i + 1])
                ? char.ConvertToUtf32(s[i], s[++i])
                : s[i];
        }
        if (i < s.Length) yield return s[^1];
    }

    public static string ToStringByChars(IEnumerable<int> chars) => chars.Select(char.ConvertFromUtf32).Join();
}
