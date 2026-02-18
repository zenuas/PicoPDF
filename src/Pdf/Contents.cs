using Mina.Extension;
using PicoPDF.Binder.Element;
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

    public void DrawText(string text, double left, double basey, double size, IFont font, IColor? color = null, Rectangle? clip = null)
    {
        DrawText(
                text,
                new PointValue(left),
                new PointValue(basey),
                size,
                font,
                color,
                clip
            );
    }

    public void DrawText(string text, IPoint left, IPoint basey, double size, IFont font, IColor? color = null, Rectangle? clip = null)
    {
        if (font is IFontChars fontchars) fontchars.WriteString(text);
        var str = new DrawString()
        {
            Text = text,
            X = left,
            Y = basey,
            FontSize = size,
            Font = font,
            Color = color,
        };
        if (clip is { } r)
        {
            Operations.Add(new DrawClipping()
            {
                X = r.X,
                Y = r.Y,
                Width = r.Width,
                Height = r.Height,
                Operation = str,
            });
        }
        else
        {
            Operations.Add(str);
        }
    }

    public void DrawTextFont((string Text, Type0Font Font)[] textfonts, double left, double basey, double size, IColor? color = null, Rectangle? clip = null)
    {
        var left_shift = left;
        foreach (var (text, font) in textfonts)
        {
            var box = font.MeasureStringBox(text);
            DrawText(text, left_shift, basey, size, font, color, clip);
            left_shift += box.Width * size;
        }
    }

    public double DrawMultilineText(string text, double top, double left, double size, Type0Font[] fonts, double width = 0, TextStyle style = TextStyle.None, TextAlignment alignment = TextAlignment.Start, IColor? color = null)
    {
        var linetop = top;
        double? prev_linegap = null;
        var max_width = 0.0;
        var max_height = 0.0;
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
            var rect = !style.HasFlag(TextStyle.Clipping) ? (Rectangle?)null : new Rectangle()
            {
                X = new PointValue(left),
                Y = new PointValue(linetop),
                Width = new PointValue(width),
                Height = new PointValue(text_height),
            };

            DrawTextFont(textfonts, text_left, basey, text_size, color, rect);
            if ((style & TextStyle.TextStyleMask) > 0) DrawTextStyle(style, linetop, text_left, basey, text_width, text_height, color);
            linetop += text_height;
            prev_linegap = allbox.LineGap * text_size;
            max_width = Math.Max(max_width, text_width);
            max_height = Math.Max(max_height, text_height);
        }
        if ((style & TextStyle.BorderStyleMask) > 0) DrawBorderStyle(style, top, left, width > 0 ? width : max_width, linetop - top, max_height / 20, color);
        return linetop - top;
    }

    public void DrawTextStyle(TextStyle style, double top, double left, double basey, double width, double height, IColor? color = null)
    {
        var right = left + width;
        var linewidth = height / 20;

        if (style.HasFlag(TextStyle.Underline))
        {
            DrawLine(left, basey + (linewidth * 2), right, basey + (linewidth * 2), color, linewidth * 2);
        }
        if (style.HasFlag(TextStyle.DoubleUnderline))
        {
            DrawLine(left, basey + linewidth, right, basey + linewidth, color, linewidth);
            DrawLine(left, basey + (linewidth * 3), right, basey + (linewidth * 3), color, linewidth);
        }
        if ((style & (TextStyle.Strikethrough | TextStyle.DoubleStrikethrough)) > 0)
        {
            var center = top + (height / 2);
            if (style.HasFlag(TextStyle.Strikethrough)) DrawLine(left, center, right, center, color, linewidth);
            if (style.HasFlag(TextStyle.DoubleStrikethrough))
            {
                DrawLine(left, center + linewidth, right, center + linewidth, color, linewidth);
                DrawLine(left, center - linewidth, right, center - linewidth, color, linewidth);
            }
        }
    }

    public void DrawBorderStyle(TextStyle style, double top, double left, double width, double height, double? linewidth = null, IColor? color = null)
    {
        var bottom = top + height;
        var right = left + width;
        linewidth ??= height / 20;

        if (style.HasFlag(TextStyle.Border))
        {
            DrawRectangle(left, top, width, height, color, linewidth);
        }
        if ((style & (TextStyle.BorderTop | TextStyle.BorderBottom)) == 0 || (style & (TextStyle.BorderLeft | TextStyle.BorderRight)) == 0)
        {
            // draw multi strokes
            if (style.HasFlag(TextStyle.BorderTop))
            {
                DrawLine(left, top, right, top, color, linewidth);
            }
            if (style.HasFlag(TextStyle.BorderBottom))
            {
                DrawLine(left, bottom, right, bottom, color, linewidth);
            }
            if (style.HasFlag(TextStyle.BorderLeft))
            {
                DrawLine(left, top, left, bottom, color, linewidth);
            }
            if (style.HasFlag(TextStyle.BorderRight))
            {
                DrawLine(right, top, right, bottom, color, linewidth);
            }
        }
        else
        {
            // draw one stroke
            switch (style & TextStyle.BorderStyleMask)
            {
                case TextStyle.BorderTop | TextStyle.BorderRight: DrawLines([(left, top), (right, top), (right, bottom)], color, linewidth); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom: DrawLines([(right, top), (right, bottom), (left, bottom)], color, linewidth); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft: DrawLines([(right, bottom), (left, bottom), (left, top)], color, linewidth); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop: DrawLines([(left, bottom), (left, top), (right, top)], color, linewidth); break;

                case TextStyle.BorderTop | TextStyle.BorderRight | TextStyle.BorderBottom: DrawLines([(left, top), (right, top), (right, bottom), (left, bottom)], color, linewidth); break;
                case TextStyle.BorderRight | TextStyle.BorderBottom | TextStyle.BorderLeft: DrawLines([(right, top), (right, bottom), (left, bottom), (left, top)], color, linewidth); break;
                case TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderTop: DrawLines([(right, bottom), (left, bottom), (left, top), (right, top)], color, linewidth); break;
                case TextStyle.BorderLeft | TextStyle.BorderTop | TextStyle.BorderRight: DrawLines([(left, bottom), (left, top), (right, top), (right, bottom)], color, linewidth); break;
            }
        }
    }

    public void DrawLine(double start_x, double start_y, double end_x, double end_y, IColor? color = null, double? linewidth = null)
    {
        DrawLines(
                [(new PointValue(start_x), new PointValue(start_y)), (new PointValue(end_x), new PointValue(end_y))],
                color,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawLines((double X, double Y)[] points, IColor? color = null, double? linewidth = null)
    {
        DrawLines(
                [.. points.Select(x => (new PointValue(x.X), new PointValue(x.Y)))],
                color,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawLine(IPoint start_x, IPoint start_y, IPoint end_x, IPoint end_y, IColor? color = null, IPoint? linewidth = null)
    {
        DrawLines(
                [(start_x, start_y), (end_x, end_y)],
                color,
                linewidth
            );
    }

    public void DrawLines((IPoint X, IPoint Y)[] points, IColor? color = null, IPoint? linewidth = null)
    {
        Operations.Add(new DrawLine()
        {
            Points = points,
            Color = color,
            LineWidth = linewidth ?? new PointValue(1),
        });
    }

    public void DrawRectangle(double x, double y, double width, double height, IColor? color = null, double? linewidth = null)
    {
        DrawRectangle(
                new PointValue(x),
                new PointValue(y),
                new PointValue(width),
                new PointValue(height),
                color,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawRectangle(IPoint x, IPoint y, IPoint width, IPoint height, IColor? color = null, IPoint? linewidth = null)
    {
        Operations.Add(new DrawRectangle()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Color = color,
            LineWidth = linewidth ?? new PointValue(1),
        });
    }

    public void DrawFillRectangle(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null)
    {
        DrawFillRectangle(
                new PointValue(x),
                new PointValue(y),
                new PointValue(width),
                new PointValue(height),
                line,
                fill,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawFillRectangle(IPoint x, IPoint y, IPoint width, IPoint height, IColor? line = null, IColor? fill = null, IPoint? linewidth = null)
    {
        Operations.Add(new DrawFillRectangle()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            LineColor = line,
            FillColor = fill,
            LineWidth = linewidth ?? new PointValue(1),
        });
    }

    public void DrawImage(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0)
    {
        DrawImage(
                new PointValue(x),
                new PointValue(y),
                image,
                zoomwidth,
                zoomheight
            );
    }

    public void DrawImage(IPoint x, IPoint y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0)
    {
        Operations.Add(new DrawImage()
        {
            X = x,
            Y = y,
            Image = image,
            ZoomWidth = zoomwidth,
            ZoomHeight = zoomheight,
        });
    }

    public override void DoExport(PdfExportOption option)
    {
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        Operations.Each(x => x.OperationWrite(Page.Width, Page.Height, writer, option));
        writer.Flush();
    }
}
