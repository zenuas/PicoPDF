using Binder;
using Mina.Extension;
using OpenType;
using PicoPDF.Loader;
using PicoPDF.Loader.Sections;
using PicoPDF.Model;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Documents;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Numerics;
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

    public static readonly Matrix3x2 FlipY = Matrix3x2.CreateScale(1, -1);

    public static string ToEscapeString(string s, Encoding encoding) => s.All(char.IsAscii) ? $"({s.ReplaceBeforeInsert(EscapeChars, ['\\']).ToStringByChars()})" : ToHexString(s, encoding);

    public static string ToHexString(string s, Encoding encoding) => $"<{Convert.ToHexStringLower(encoding.GetBytes(s))}>";

    public static DeviceRGB ToDeviceRGB(this System.Drawing.Color color) => new((double)color.R / 255, (double)color.G / 255, (double)color.B / 255);

    public static Document CreateDocument<T>(string json, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null, PdfEventOption? option = null) => CreateDocument(JsonLoader.CreatePageFromJsonFile(json, option), datas, mapper, register, option);
    public static Document CreateDocument(string json, DataTable table, IFontRegister? register = null, PdfEventOption? option = null) => CreateDocument(JsonLoader.CreatePageFromJsonFile(json, option), table, register, option);
    public static Document CreateDocument(string json, DataView view, IFontRegister? register = null, PdfEventOption? option = null) => CreateDocument(JsonLoader.CreatePageFromJsonFile(json, option), view, register, option);
    public static Document CreateDocument<T>(PageSection pagesection, IEnumerable<T> datas, Dictionary<string, Func<T, object>>? mapper = null, IFontRegister? register = null, PdfEventOption? option = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind<T, PageModel, SectionModel>(pagesection, datas, mapper), option ?? new()));
    public static Document CreateDocument(PageSection pagesection, DataTable table, IFontRegister? register = null, PdfEventOption? option = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind<PageModel, SectionModel>(pagesection, table), option ?? new()));
    public static Document CreateDocument(PageSection pagesection, DataView view, IFontRegister? register = null, PdfEventOption? option = null) => new Document { FontRegister = register ?? CreateDefaultFontRegister() }.Return(x => ModelMapping.Mapping(x, SectionBinder.Bind<PageModel, SectionModel>(pagesection, view), option ?? new()));

    public static IFontRegister CreateDefaultFontRegister() => new FontRegister().Return(x => x.RegisterDirectory([.. FontRegister.GetFontDirectories()]));

    public static IEnumerable<(string Text, Type0Font Font)[]> GetMultilineTextFont(string text, Type0Font[] fonts, double size, double width, ILineBreakRule linebreak_rule)
    {
        foreach (var line in text.SplitLine())
        {
            foreach (var textfonts in GetTextFont(line, fonts, size, width, linebreak_rule)) yield return textfonts;
        }
    }

    public static IEnumerable<(string Text, Type0Font Font)[]> GetTextFont(string line, Type0Font[] fonts, double size, double width, ILineBreakRule linebreak_rule)
    {
        if (line.Length == 0) yield break;

        var charfonts = line.ToUtf32CharArray().Select(x => (Char: x, Font: GetTextFont(x, fonts))).ToArray();
        var textfonts = new List<(string Text, Type0Font Font)>();
        var prev_font = charfonts[0].Font;
        var prev_text = new List<int>() { charfonts[0].Char };
        var total_width = charfonts[0].Font.Font.MeasureChar(charfonts[0].Char) * size;
        for (var i = 1; i < charfonts.Length; i++)
        {
            var char_width = charfonts[i].Font.Font.MeasureChar(charfonts[i].Char) * size;
            if (width > 0 && total_width + char_width > width)
            {
                if (linebreak_rule.DenyStartChar.Contains(charfonts[i].Char) || linebreak_rule.DenyEndChar.Contains(charfonts[i - 1].Char))
                {
                    if (prev_text.Count > 1) textfonts.Add((prev_text[0..^1].ToStringByChars(), prev_font));
                    i--;
                    total_width = charfonts[i].Font.Font.MeasureChar(charfonts[i].Char) * size;
                }
                else
                {
                    textfonts.Add((prev_text.ToStringByChars(), prev_font));
                    total_width = char_width;
                }
                yield return [.. textfonts];
                textfonts.Clear();

                prev_font = charfonts[i].Font;
                prev_text.Clear();
                prev_text.Add(charfonts[i].Char);
            }
            else
            {
                if (ReferenceEquals(prev_font, charfonts[i].Font))
                {
                    prev_text.Add(charfonts[i].Char);
                }
                else
                {
                    textfonts.Add((prev_text.ToStringByChars(), prev_font));
                    prev_font = charfonts[i].Font;
                    prev_text.Clear();
                    prev_text.Add(charfonts[i].Char);
                }
                total_width += char_width;
            }
        }
        textfonts.Add((prev_text.ToStringByChars(), prev_font));
        yield return [.. textfonts];
    }

    public static Type0Font GetTextFont(int c, Type0Font[] fonts) => fonts.Where(x => x.Font.CharToGID(c) > 0).FirstOrDefault() ?? fonts[0];

    public static FontBox MeasureTextFontBox((string Text, Type0Font Font)[] textfonts) => textfonts
        .Select(x => MeasureStringBox(x.Font.Font, x.Text))
        .Aggregate(new FontBox(), (acc, x) => new(Math.Min(acc.Ascender, x.Ascender), Math.Max(acc.Descender, x.Descender), Math.Max(acc.LineGap, x.LineGap), acc.Width + x.Width));

    public static FontBox MeasureStringBox(IOpenTypeFont font, string s) => new()
    {
        Ascender = (double)-(font.OS2?.STypoAscender ?? font.HorizontalHeader.Ascender) / font.FontHeader.UnitsPerEm,
        Descender = (double)-(font.OS2?.STypoDescender ?? font.HorizontalHeader.Descender) / font.FontHeader.UnitsPerEm,
        LineGap = (double)font.HorizontalHeader.LineGap / font.FontHeader.UnitsPerEm,
        Width = font.MeasureString(s),
    };

    public static string PointToString((IPoint X, IPoint Y) point, int height, string format) => $"{PointToString(point.X.ToPoint(), format)} {PointToString(height - point.Y.ToPoint(), format)}";

    public static string PointToString(double point, string format) =>
        format == "F%" ? point.ToString("F7", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.') :
            point <= long.MaxValue &&
            point >= long.MinValue &&
            point % 1d == 0d ? ((long)point).ToString() : point.ToString(format, CultureInfo.InvariantCulture);
}
