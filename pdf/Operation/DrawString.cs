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

    public static readonly NoneLineBreakRule NoneLineBreakRule = new();

    public static ILineBreakRule GetLineBreakRule(TextStyles style)
    {
        if (!style.HasBit(TextStyles.BreakMask)) return NoneLineBreakRule;

        var deny_start_char = new HashSet<int>();
        var deny_end_char = new HashSet<int>();

        if (style.HasFlag(TextStyles.LineBreakSimplifiedChinese))
        {
            SimplifiedChineseLineBreakRule.StaticDenyStartChar.Each(x => deny_start_char.Add(x));
            SimplifiedChineseLineBreakRule.StaticDenyEndChar.Each(x => deny_end_char.Add(x));
        }

        if (style.HasFlag(TextStyles.LineBreakTraditionalChinese))
        {
            TraditionalChineseLineBreakRule.StaticDenyStartChar.Each(x => deny_start_char.Add(x));
            TraditionalChineseLineBreakRule.StaticDenyEndChar.Each(x => deny_end_char.Add(x));
        }

        if (style.HasFlag(TextStyles.LineBreakJapanese))
        {
            JapaneseLineBreakRule.StaticDenyStartChar.Each(x => deny_start_char.Add(x));
            JapaneseLineBreakRule.StaticDenyEndChar.Each(x => deny_end_char.Add(x));
        }

        if (style.HasFlag(TextStyles.LineBreakKorean))
        {
            KoreanLineBreakRule.StaticDenyStartChar.Each(x => deny_start_char.Add(x));
            KoreanLineBreakRule.StaticDenyEndChar.Each(x => deny_end_char.Add(x));
        }

        return new NoneLineBreakRule()
        {
            DenyStartChar = deny_start_char,
            DenyEndChar = deny_end_char,
        };
    }

    public static IOperation Create(string text, double left, double top, double size, Type0Font[] fonts, Document document, double width = 0, double height = 0, TextStyles style = TextStyles.None, TextAlignments alignment = TextAlignments.Start, IColor? color = null, ILineBreakRule? linebreak_rule = null)
    {
        var opt = fonts.First().FontLoadOption;
        return
            opt.HasFlag(FontLoadOptions.VerticalLeftToRight) ? CreateVerticalLeftToRight(text, left, top, size, fonts, document, width, height, style, alignment, color, linebreak_rule) :
            opt.HasFlag(FontLoadOptions.VerticalRightToLeft) ? CreateVerticalRightToLeft(text, left, top, size, fonts, document, width, height, style, alignment, color, linebreak_rule) :
            CreateHorizontalLeftToRight(text, left, top, size, fonts, document, width, height, style, alignment, color, linebreak_rule);
    }

    public static IOperation CreateHorizontalLeftToRight(string text, double left, double top, double size, Type0Font[] fonts, Document document, double width = 0, double height = 0, TextStyles style = TextStyles.None, TextAlignments alignment = TextAlignments.Start, IColor? color = null, ILineBreakRule? linebreak_rule = null)
    {
        var linetop = top;
        double? prev_linegap = null;
        var max_width = 0.0;
        var max_height = 0.0;
        var opes = new List<IOperation>();
        foreach (var textfonts in GetMultilineTextFont(text, fonts, size, style.HasFlag(TextStyles.MultiLine) ? width : 0, linebreak_rule ?? GetLineBreakRule(style)))
        {
            if (prev_linegap is { } gap) linetop += gap;

            var allbox = MeasureTextFontBox(textfonts, MeasureHorizontalStringBox);
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

            opes.AddRange(CreateHorizontalMultilineText(textfonts, basey, text_left, text_size, style.HasFlag(TextStyles.Stroke), document, color));
            if (style.HasBit(TextStyles.TextStyleMask)) opes.AddRange(DrawOperations.CreateTextStyle(style, linetop, text_left, basey, text_width, text_height, color));
            linetop += text_height;
            prev_linegap = allbox.LineGap * text_size;
            max_width = Math.Max(max_width, text_width);
            max_height = Math.Max(max_height, text_height);
        }
        if (style.HasBit(TextStyles.BorderStyleMask)) opes.AddRange(DrawOperations.CreateBorderStyle(style, top, left, width > 0 ? width : max_width, height > 0 ? height : linetop - top, max_height / 20, color));

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

    public static IOperation CreateVerticalLeftToRight(string text, double left, double top, double size, Type0Font[] fonts, Document document, double width = 0, double height = 0, TextStyles style = TextStyles.None, TextAlignments alignment = TextAlignments.Start, IColor? color = null, ILineBreakRule? linebreak_rule = null)
    {
        var lineleft = left;
        double? prev_linegap = null;
        var max_width = 0.0;
        var max_height = 0.0;
        var opes = new List<IOperation>();
        foreach (var textfonts in GetMultilineTextFont(text, fonts, size, style.HasFlag(TextStyles.MultiLine) ? width : 0, linebreak_rule ?? GetLineBreakRule(style)))
        {
            if (prev_linegap is { } gap) lineleft += gap;

            var allbox = MeasureTextFontBox(textfonts, MeasureVerticalStringBox);
            var firstchar = MeasureVerticalStringBox(textfonts[0].Font.Font, textfonts[0].Text.ToUtf32CharArray().First());
            var text_size = style.HasFlag(TextStyles.ShrinkToFit) && height < (allbox.Width * size) ? height / allbox.Width : size;
            var text_width = allbox.Height * text_size;
            var text_height = allbox.Width * text_size;
            var text_center = lineleft + (text_width / 2);
            var text_top = (firstchar.Width * text_size) + alignment switch
            {
                TextAlignments.Center => top + ((height - text_height) / 2),
                TextAlignments.End => top + height - text_height,
                _ => top,
            };

            opes.AddRange(CreateVerticalMultilineText(textfonts, text_top, text_center, text_size, style.HasFlag(TextStyles.Stroke), document, color));
            if (style.HasBit(TextStyles.TextStyleMask)) opes.AddRange(DrawOperations.CreateTextStyle(style, text_top, text_center, text_top, text_width, text_height, color));
            lineleft += text_width;
            prev_linegap = allbox.LineGap * text_size;
            max_width = Math.Max(max_width, text_width);
            max_height = Math.Max(max_height, text_height);
        }
        if (style.HasBit(TextStyles.BorderStyleMask)) opes.AddRange(DrawOperations.CreateBorderStyle(style, top, left, width > 0 ? width : lineleft, height > 0 ? height : max_height, max_width / 20, color));

        return !style.HasFlag(TextStyles.Clipping) ?
            new DrawOperations
            {
                X = new PointValue(left),
                Y = new PointValue(top),
                Width = new PointValue(width > 0 ? width : lineleft),
                Height = new PointValue(height > 0 ? height : max_height),
                Operations = [.. opes]
            } :
            new DrawClipping
            {
                X = new PointValue(left),
                Y = new PointValue(top),
                Width = new PointValue(width),
                Height = new PointValue(height > 0 ? height : max_height),
                Operations = [.. opes],
            };
    }

    public static IOperation CreateVerticalRightToLeft(string text, double left, double top, double size, Type0Font[] fonts, Document document, double width = 0, double height = 0, TextStyles style = TextStyles.None, TextAlignments alignment = TextAlignments.Start, IColor? color = null, ILineBreakRule? linebreak_rule = null)
    {
        throw new();
    }

    public static IEnumerable<IOperation> CreateHorizontalMultilineText((string Text, Type0Font Font)[] textfonts, double basey, double left, double size, bool stroke, Document document, IColor? color = null)
    {
        var start = left;
        foreach (var (text, font) in textfonts)
        {
            var box = MeasureHorizontalStringBox(font.Font, text);
            if (stroke || (font.FontLoadOption & FontLoadOptions.EmbedsMask) == FontLoadOptions.Stroke)
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

    public static IEnumerable<IOperation> CreateVerticalMultilineText((string Text, Type0Font Font)[] textfonts, double basey, double left, double size, bool stroke, Document document, IColor? color = null)
    {
        var start = basey;
        foreach (var (text, font) in textfonts)
        {
            var box = MeasureVerticalStringBox(font.Font, text);
            if (stroke || (font.FontLoadOption & FontLoadOptions.EmbedsMask) == FontLoadOptions.Stroke)
            {
                foreach (var op in DrawPathOperations.CreateStringToPath(text, start, left, size, font, document, color)) yield return op;
            }
            else
            {
                yield return Create(text, start, left, size, font, color);
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
        var total_width = charfonts[0].Font.Font.HorizontalMeasureChar(charfonts[0].Char) * size;
        for (var i = 1; i < charfonts.Length; i++)
        {
            var char_width = charfonts[i].Font.Font.HorizontalMeasureChar(charfonts[i].Char) * size;
            if (width > 0 && total_width + char_width > width)
            {
                if (linebreak_rule.DenyStartChar.Contains(charfonts[i].Char) || linebreak_rule.DenyEndChar.Contains(charfonts[i - 1].Char))
                {
                    if (prev_text.Count > 1) textfonts.Add((prev_text[0..^1].ToStringByChars(), prev_font));
                    i--;
                    total_width = charfonts[i].Font.Font.HorizontalMeasureChar(charfonts[i].Char) * size;
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

    public static FontBox MeasureTextFontBox((string Text, Type0Font Font)[] textfonts, Func<IOpenTypeFont, string, FontBox> measure) => textfonts
        .Select(x => measure(x.Font.Font, x.Text))
        .Aggregate(new FontBox(0, 0, 0, 0), (acc, x) => new(Math.Min(acc.Ascender, x.Ascender), Math.Max(acc.Descender, x.Descender), Math.Max(acc.LineGap, x.LineGap), acc.Width + x.Width));

    public static FontBox MeasureHorizontalStringBox(IOpenTypeFont font, string s) => new(
            (double)-(font.OS2?.STypoAscender.Value ?? font.HorizontalHeader.Ascender.Value) / font.FontHeader.UnitsPerEm,
            (double)-(font.OS2?.STypoDescender.Value ?? font.HorizontalHeader.Descender.Value) / font.FontHeader.UnitsPerEm,
            (double)font.HorizontalHeader.LineGap.Value / font.FontHeader.UnitsPerEm,
            font.HorizontalMeasureString(s)
        );

    public static FontBox MeasureVerticalStringBox(IOpenTypeFont font, string s) => new(
            (double)-(font.VerticalHeader?.Ascent.Value ?? 0) / font.FontHeader.UnitsPerEm,
            (double)-(font.VerticalHeader?.Descent.Value ?? 0) / font.FontHeader.UnitsPerEm,
            (double)(font.VerticalHeader?.LineGap.Value ?? 0) / font.FontHeader.UnitsPerEm,
            font.VerticalMeasureString(s)
        );

    public static FontBox MeasureVerticalStringBox(IOpenTypeFont font, int c) => new(
            (double)-(font.VerticalHeader?.Ascent.Value ?? 0) / font.FontHeader.UnitsPerEm,
            (double)-(font.VerticalHeader?.Descent.Value ?? 0) / font.FontHeader.UnitsPerEm,
            (double)(font.VerticalHeader?.LineGap.Value ?? 0) / font.FontHeader.UnitsPerEm,
            font.VerticalMeasureChar(c)
        );
}
