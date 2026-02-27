using Mina.Extension;
using PicoPDF.Loader.Elements;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.Operation;
using PicoPDF.Pdf.XObject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf;

public class Contents : PdfObject
{
    public required Page Page { get; init; }
    public List<IOperation> Operations { get; init; } = [];

    public void DrawTextOnBaseline(string text, double basey, double left, double size, IFont font) => Operations.Add(CreateDrawTextOnBaselineOperation(text, basey, left, size, font));

    public static DrawString CreateDrawTextOnBaselineOperation(string text, double basey, double left, double size, IFont font, IColor? color = null) => CreateDrawTextOnBaselineOperation(
            text,
            new PointValue(basey),
            new PointValue(left),
            size,
            font,
            color
        );

    public static DrawString CreateDrawTextOnBaselineOperation(string text, IPoint basey, IPoint left, double size, IFont font, IColor? color = null)
    {
        if (font is IFontChars fontchars) fontchars.AddCharCache(text);
        return new()
        {
            Text = text,
            X = left,
            Y = basey,
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
        foreach (var textfonts in PdfUtility.GetMultilineTextFont(text, fonts))
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

            opes.AddRange(CreateDrawTextOperation(textfonts, basey, text_left, text_size, color));
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

    public static IEnumerable<DrawString> CreateDrawTextOperation((string Text, Type0Font Font)[] textfonts, double basey, double left, double size, IColor? color = null)
    {
        var start = left;
        foreach (var (text, font) in textfonts)
        {
            var box = font.MeasureStringBox(text);
            yield return CreateDrawTextOnBaselineOperation(text, basey, start, size, font, color);
            start += box.Width * size;
        }
    }

    public void DrawTextStyle(TextStyle style, double top, double left, double basey, double width, double height, IColor? color = null) => CreateDrawTextStyleOperations(style, top, left, basey, width, height, color).Each(Operations.Add);

    public static IEnumerable<IOperation> CreateDrawTextStyleOperations(TextStyle style, double top, double left, double basey, double width, double height, IColor? color = null)
    {
        var right = left + width;
        var linewidth = height / 20;

        if (style.HasFlag(TextStyle.Underline))
        {
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 2)), (right, basey + (linewidth * 2))], color, linewidth * 2);
        }
        if (style.HasFlag(TextStyle.DoubleUnderline))
        {
            yield return CreateDrawLinesOperation([(left, basey + linewidth), (right, basey + linewidth)], color, linewidth);
            yield return CreateDrawLinesOperation([(left, basey + (linewidth * 3)), (right, basey + (linewidth * 3))], color, linewidth);
        }
        if ((style & (TextStyle.Strikethrough | TextStyle.DoubleStrikethrough)) > 0)
        {
            var center = top + (height / 2);
            if (style.HasFlag(TextStyle.Strikethrough)) yield return CreateDrawLinesOperation([(left, center), (right, center)], color, linewidth);
            if (style.HasFlag(TextStyle.DoubleStrikethrough))
            {
                yield return CreateDrawLinesOperation([(left, center + linewidth), (right, center + linewidth)], color, linewidth);
                yield return CreateDrawLinesOperation([(left, center - linewidth), (right, center - linewidth)], color, linewidth);
            }
        }
    }

    public void DrawBorderStyle(TextStyle style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null) => CreateDrawBorderStyleOperations(style, top, left, width, height, linewidth, color).Each(Operations.Add);

    public static IEnumerable<IOperation> CreateDrawBorderStyleOperations(TextStyle style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null)
    {
        var bottom = top + height;
        var right = left + width;
        linewidth ??= height / 20;

        if (style.HasFlag(TextStyle.Border))
        {
            yield return CreateDrawRectangleOperation(left, top, width, height, color, linewidth);
        }
        if ((style & (TextStyle.BorderTop | TextStyle.BorderBottom)) == 0)
        {
            if (style.HasFlag(TextStyle.BorderLeft)) yield return CreateDrawLinesOperation([(left, top), (left, bottom)], color, linewidth);
            if (style.HasFlag(TextStyle.BorderRight)) yield return CreateDrawLinesOperation([(right, top), (right, bottom)], color, linewidth);
        }
        else if ((style & (TextStyle.BorderLeft | TextStyle.BorderRight)) == 0)
        {
            if (style.HasFlag(TextStyle.BorderTop)) yield return CreateDrawLinesOperation([(left, top), (right, top)], color, linewidth);
            if (style.HasFlag(TextStyle.BorderBottom)) yield return CreateDrawLinesOperation([(left, bottom), (right, bottom)], color, linewidth);
        }
        else
        {
            // draw one stroke
            switch (style & TextStyle.BorderStyleMask)
            {
                case TextStyle.BorderTop | TextStyle.BorderRight: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom)], color, linewidth); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom)], color, linewidth); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top)], color, linewidth); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top)], color, linewidth); break;

                case TextStyle.BorderTop | TextStyle.BorderRight | TextStyle.BorderBottom: yield return CreateDrawLinesOperation([(left, top), (right, top), (right, bottom), (left, bottom)], color, linewidth); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom | TextStyle.BorderLeft: yield return CreateDrawLinesOperation([(right, top), (right, bottom), (left, bottom), (left, top)], color, linewidth); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderTop: yield return CreateDrawLinesOperation([(right, bottom), (left, bottom), (left, top), (right, top)], color, linewidth); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop | TextStyle.BorderRight: yield return CreateDrawLinesOperation([(left, bottom), (left, top), (right, top), (right, bottom)], color, linewidth); break;
            }
        }
    }

    public void DrawLine(double start_x, double start_y, double end_x, double end_y, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawLinesOperation([(start_x, start_y), (end_x, end_y)], color, linewidth));

    public void DrawLines((double X, double Y)[] points, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawLinesOperation(points, color, linewidth));

    public static DrawLine CreateDrawLinesOperation((double X, double Y)[] points, IColor? color = null, double? linewidth = null) => CreateDrawLinesOperation(
            [.. points.Select(x => (new PointValue(x.X), new PointValue(x.Y)))],
            color,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawLine CreateDrawLinesOperation((IPoint X, IPoint Y)[] points, IColor? color = null, IPoint? linewidth = null) => new()
    {
        Points = points,
        Color = color,
        LineWidth = linewidth ?? new PointValue(1),
    };

    public void DrawRectangle(double x, double y, double width, double height, IColor? color = null, double? linewidth = null) => Operations.Add(CreateDrawRectangleOperation(x, y, width, height, color, linewidth));

    public static DrawRectangle CreateDrawRectangleOperation(double x, double y, double width, double height, IColor? color = null, double? linewidth = null) => CreateDrawRectangleOperation(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            color,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawRectangle CreateDrawRectangleOperation(IPoint x, IPoint y, IPoint width, IPoint height, IColor? color = null, IPoint? linewidth = null) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        Color = color,
        LineWidth = linewidth ?? new PointValue(1),
    };

    public void DrawFillRectangle(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null) => Operations.Add(CreateDrawFillRectangleOperation(x, y, width, height, line, fill, linewidth));

    public static DrawFillRectangle CreateDrawFillRectangleOperation(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null) => CreateDrawFillRectangleOperation(
            new PointValue(x),
            new PointValue(y),
            new PointValue(width),
            new PointValue(height),
            line,
            fill,
            linewidth is { } lw ? new PointValue(lw) : null
        );

    public static DrawFillRectangle CreateDrawFillRectangleOperation(IPoint x, IPoint y, IPoint width, IPoint height, IColor? line = null, IColor? fill = null, IPoint? linewidth = null) => new()
    {
        X = x,
        Y = y,
        Width = width,
        Height = height,
        LineColor = line,
        FillColor = fill,
        LineWidth = linewidth ?? new PointValue(1),
    };

    public void DrawImage(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => Operations.Add(CreateDrawImageOperation(x, y, image, zoomwidth, zoomheight));

    public static DrawImage CreateDrawImageOperation(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => CreateDrawImageOperation(
        new PointValue(x),
        new PointValue(y),
        image,
        zoomwidth,
        zoomheight
    );

    public static DrawImage CreateDrawImageOperation(IPoint x, IPoint y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0) => new()
    {
        X = x,
        Y = y,
        Image = image,
        ZoomWidth = zoomwidth,
        ZoomHeight = zoomheight,
    };

    public override void DoExport(PdfExportOption option)
    {
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        Operations.Each(x => x.OperationWrite(Page.Width, Page.Height, writer, option));
        writer.Flush();
    }
}
