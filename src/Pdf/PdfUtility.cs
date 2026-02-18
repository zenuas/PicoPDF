using Mina.Extension;
using PicoPDF.Binder;
using PicoPDF.Binder.Data;
using PicoPDF.Model;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
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

    public static IEnumerable<(string Text, Type0Font Font)> GetTextFont(string s, Type0Font[] fonts)
    {
        if (s.Length == 0) yield break;

        var xs = s.ToUtf32CharArray().Select(x => (Char: x, Font: GetTextFont(x, fonts))).ToArray();
        var prev_font = xs[0].Font;
        var prev_text = new List<int>() { xs[0].Char };
        for (var i = 1; i < xs.Length; i++)
        {
            if (ReferenceEquals(prev_font, xs[i].Font))
            {
                prev_text.Add(xs[i].Char);
            }
            else
            {
                yield return (prev_text.ToStringByChars(), prev_font);
                prev_font = xs[i].Font;
                prev_text.Clear();
                prev_text.Add(xs[i].Char);
            }
        }
        yield return (prev_text.ToStringByChars(), prev_font);
    }

    public static Type0Font GetTextFont(int c, Type0Font[] fonts) => fonts.Where(x => x.Font.CharToGID(c) > 0).FirstOrDefault() ?? fonts[0];

    public static FontBox MeasureTextFontBox((string Text, Type0Font Font)[] textfonts) => textfonts
        .Select(x => x.Font.MeasureStringBox(x.Text))
        .Aggregate(new FontBox(), (acc, x) => new(Math.Min(acc.Ascender, x.Ascender), Math.Max(acc.Descender, x.Descender), acc.Width + x.Width));
}
