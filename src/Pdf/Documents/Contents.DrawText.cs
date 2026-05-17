using PicoPDF.Loader.Elements;
using PicoPDF.Loader.Sections;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.Operation;
using System;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

public partial class Contents
{
    public void DrawTextOnBaseline(string text, double basey, double left, double size, IFont font) => Operations.Add(CreateDrawTextOnBaselineOperation(text, basey, left, size, font));

    public static DrawString CreateDrawTextOnBaselineOperation(string text, double basey, double left, double size, IFont font, IColor? color = null)
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

    public double DrawText(string text, double top, double left, double size, Type0Font[] fonts, double width = 0, double height = 0, TextStyle style = TextStyle.None, TextAlignment alignment = TextAlignment.Start, IColor? color = null)
    {
        var linetop = top;
        double? prev_linegap = null;
        var max_width = 0.0;
        var max_height = 0.0;
        var opes = new List<IOperation>();
        foreach (var textfonts in PdfUtility.GetMultilineTextFont(text, fonts, size, style.HasFlag(TextStyle.MultiLine) ? width : 0))
        {
            if (prev_linegap is { } gap) linetop += gap;

            var allbox = PdfUtility.MeasureTextFontBox(textfonts);
            var text_size = style.HasFlag(TextStyle.ShrinkToFit) && width < (allbox.Width * size) ? width / allbox.Width : size;
            var text_width = allbox.Width * text_size;
            var text_height = allbox.Height * text_size;
            var basey = linetop - (allbox.Ascender * text_size);
            var text_left = alignment switch
            {
                TextAlignment.Center => left + ((width - text_width) / 2),
                TextAlignment.End => left + width - text_width,
                _ => left,
            };

            opes.AddRange(CreateDrawTextOperation(textfonts, basey, text_left, text_size, style.HasFlag(TextStyle.Stroke), Page.Document, color));
            if ((style & TextStyle.TextStyleMask) > 0) opes.AddRange(CreateDrawTextStyleOperations(style, linetop, text_left, basey, text_width, text_height, color));
            linetop += text_height;
            prev_linegap = allbox.LineGap * text_size;
            max_width = Math.Max(max_width, text_width);
            max_height = Math.Max(max_height, text_height);
        }
        if ((style & TextStyle.BorderStyleMask) > 0) opes.AddRange(CreateDrawBorderStyleOperations(style, top, left, width > 0 ? width : max_width, height > 0 ? height : linetop - top, max_height / 20, color));

        if (style.HasFlag(TextStyle.Clipping))
        {
            Operations.Add(new DrawClipping()
            {
                X = new PointValue(left),
                Y = new PointValue(top),
                Width = new PointValue(width),
                Height = new PointValue(height > 0 ? height : linetop - top),
                Operations = [.. opes],
            });
        }
        else
        {
            Operations.AddRange(opes);
        }
        return linetop - top;
    }

    public static IEnumerable<IOperation> CreateDrawTextOperation((string Text, Type0Font Font)[] textfonts, double basey, double left, double size, bool stroke, Document document, IColor? color = null)
    {
        var start = left;
        foreach (var (text, font) in textfonts)
        {
            var box = PdfUtility.MeasureStringBox(font.Font, text);
            if (stroke || font.FontEmbed == FontEmbed.Stroke)
            {
                foreach (var op in CreateDrawPathOnBaselineOperation(text, basey, start, size, font, document, color)) yield return op;
            }
            else
            {
                yield return CreateDrawTextOnBaselineOperation(text, basey, start, size, font, color);
            }
            start += box.Width * size;
        }
    }
}
