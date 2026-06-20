using Mina.Extension;
using OpenType;
using Pdf.Documents;
using Pdf.Documents.BreakRule;
using Pdf.Drawing;
using Pdf.Extension;
using Pdf.Font;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pdf.Operation;

public class DrawString : IOperation
{
    public required string Text { get; init; }
    public required IPoint X { get; init; }
    public required IPoint Y { get; init; }
    public required IFont Font { get; init; }
    public required double FontSize { get; init; }
    public IColor? Color { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write("BT\n");
        if (Color is { } c)
        {
            writer.Write("  q\n");
            writer.Write($"  {c.CreateColor(false)}\n");
        }
        writer.Write($"  /{Font.Name} {FontSize.ToPointString(option.PointFormat)} Tf\n");
        writer.Write($"  {(X, Y).ToPointString(height, option.PointFormat)} Td\n");
        writer.Write($"  {Font.CreateTextShowingOperator(Text)}");
        if (option.Debug) writer.Write($" % {Text.ReplaceLineEndings("")}");
        writer.Write("\n");
        if (Color is { })
        {
            writer.Write("  Q\n");
        }
        writer.Write("ET\n");
    }

    public static readonly NoneLineBreakRule NoneLineBreakRule = new();
    public static readonly GenericLineBreakRule GenericLineBreakRule = new();

    public static DrawString Create(string text, double basey, double left, double size, IFont font, IColor? color = null)
    {
        if (font is IFontChars fontchars) fontchars.AddCharCache(text);
        return new()
        {
            Text = text,
            X = new PointValue(left),
            Y = new PointValue(basey),
            FontSize = size,
            Font = font,
            Color = color,
        };
    }

    public static IOperation Create(Document document, string text, double left, double top, double size, Type0Font[] fonts, double width = 0, double height = 0, TextStyles style = TextStyles.None, TextAlignments alignment = TextAlignments.Start, IColor? color = null, ILineBreakRule? linebreak_rule = null)
    {
        var linetop = top;
        double? prev_linegap = null;
        var max_width = 0.0;
        var max_height = 0.0;
        var opes = new List<IOperation>();
        foreach (var textfonts in GetMultilineTextFont(text, fonts, size, style.HasFlag(TextStyles.MultiLine) ? width : 0, linebreak_rule ?? (style.HasFlag(TextStyles.LineBreak) ? GenericLineBreakRule : NoneLineBreakRule)))
        {
            if (prev_linegap is { } gap) linetop += gap;

            var allbox = MeasureTextFontBox(textfonts);
            var text_size = style.HasFlag(TextStyles.ShrinkToFit) && width < (allbox.Width * size) ? width / allbox.Width : size;
            var text_width = allbox.Width * text_size;
            var text_height = allbox.Height * text_size;
            var basey = linetop - (allbox.Ascender * text_size);
            var text_left = alignment switch
            {
                TextAlignments.Center => left + ((width - text_width) / 2),
                TextAlignments.End => left + width - text_width,
                _ => left,
            };

            opes.AddRange(CreateMultilineText(textfonts, basey, text_left, text_size, style.HasFlag(TextStyles.Stroke), document, color));
            if ((style & TextStyles.TextStyleMask) > 0) opes.AddRange(Contents.CreateDrawTextStyleOperations(style, linetop, text_left, basey, text_width, text_height, color));
            linetop += text_height;
            prev_linegap = allbox.LineGap * text_size;
            max_width = Math.Max(max_width, text_width);
            max_height = Math.Max(max_height, text_height);
        }
        if ((style & TextStyles.BorderStyleMask) > 0) opes.AddRange(Contents.CreateDrawBorderStyleOperations(style, top, left, width > 0 ? width : max_width, height > 0 ? height : linetop - top, max_height / 20, color));

        return !style.HasFlag(TextStyles.Clipping) ?
            new DrawOperations
            {
                X = new PointValue(left),
                Y = new PointValue(top),
                Width = new PointValue(width > 0 ? width : max_width),
                Height = new PointValue(height > 0 ? height : linetop - top),
                Operations = [.. opes]
            } :
            new DrawClipping
            {
                X = new PointValue(left),
                Y = new PointValue(top),
                Width = new PointValue(width),
                Height = new PointValue(height > 0 ? height : linetop - top),
                Operations = [.. opes],
            };
    }

    public static IEnumerable<IOperation> CreateMultilineText((string Text, Type0Font Font)[] textfonts, double basey, double left, double size, bool stroke, Document document, IColor? color = null)
    {
        var start = left;
        foreach (var (text, font) in textfonts)
        {
            var box = MeasureStringBox(font.Font, text);
            if (stroke || font.FontEmbed == FontEmbeds.Stroke)
            {
                foreach (var op in DrawPathOperations.CreateStringToPath(text, basey, start, size, font, document, color)) yield return op;
            }
            else
            {
                yield return Create(text, basey, start, size, font, color);
            }
            start += box.Width * size;
        }
    }

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
        var prev_text = new List<int> { charfonts[0].Char };
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
        .Aggregate(new FontBox(0, 0, 0, 0), (acc, x) => new(Math.Min(acc.Ascender, x.Ascender), Math.Max(acc.Descender, x.Descender), Math.Max(acc.LineGap, x.LineGap), acc.Width + x.Width));

    public static FontBox MeasureStringBox(IOpenTypeFont font, string s) => new(
            (double)-(font.OS2?.STypoAscender ?? font.HorizontalHeader.Ascender) / font.FontHeader.UnitsPerEm,
            (double)-(font.OS2?.STypoDescender ?? font.HorizontalHeader.Descender) / font.FontHeader.UnitsPerEm,
            (double)font.HorizontalHeader.LineGap / font.FontHeader.UnitsPerEm,
            font.MeasureString(s)
        );
}
