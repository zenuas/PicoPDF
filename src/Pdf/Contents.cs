using Mina.Extension;
using PicoPDF.Binder.Element;
using PicoPDF.Pdf.Color;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.Operation;
using PicoPDF.Pdf.XObject;
using System.Collections.Generic;

namespace PicoPDF.Pdf;

public class Contents : PdfObject
{
    public required Page Page { get; init; }
    public List<IOperation> Operations { get; init; } = [];

    public void DrawString(string s, double x, double y, double size, IFont font, IColor? c = null, Rectangle? clip = null)
    {
        DrawString(
                s,
                new PointValue(x),
                new PointValue(y),
                size,
                font,
                c,
                clip
            );
    }

    public void DrawString(string s, IPoint x, IPoint y, double size, IFont font, IColor? c = null, Rectangle? clip = null)
    {
        if (font is IFontChars fontchars) fontchars.WriteString(s);
        var str = new DrawString()
        {
            Text = s,
            X = x,
            Y = y,
            FontSize = size,
            Font = font,
            Color = c,
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

    public void DrawTextStyle(TextStyle style, double posx, double posy, double basey, double width, double height, IColor? c = null)
    {
        if (style.HasFlag(TextStyle.Underline))
        {
            DrawLine(posx, basey, posx + width, basey, c);
        }
        if (style.HasFlag(TextStyle.DoubleUnderline))
        {
            DrawLine(posx, basey + 1, posx + width, basey + 1, c);
            DrawLine(posx, basey - 1, posx + width, basey - 1, c);
        }
        if ((style & (TextStyle.Strikethrough | TextStyle.DoubleStrikethrough)) > 0)
        {
            var center = posy + (height / 2);
            if (style.HasFlag(TextStyle.Strikethrough)) DrawLine(posx, center, posx + width, center, c);
            if (style.HasFlag(TextStyle.DoubleStrikethrough))
            {
                DrawLine(posx, center + 1, posx + width, center + 1, c);
                DrawLine(posx, center - 1, posx + width, center - 1, c);
            }
        }
        if (style.HasFlag(TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight))
        {
            DrawRectangle(posx, posy, width, height, c);
        }
        else if ((style & (TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight)) > 0)
        {
            if (style.HasFlag(TextStyle.BorderTop))
            {
                DrawLine(posx, posy, posx + width, posy, c);
            }
            if (style.HasFlag(TextStyle.BorderBottom))
            {
                DrawLine(posx, posy + height, posx + width, posy + height, c);
            }
            if (style.HasFlag(TextStyle.BorderLeft))
            {
                DrawLine(posx, posy, posx, posy + height, c);
            }
            if (style.HasFlag(TextStyle.BorderRight))
            {
                DrawLine(posx + width, posy, posx + width, posy + height, c);
            }
        }
    }

    public void DrawLine(double start_x, double start_y, double end_x, double end_y, IColor? c = null, double? linewidth = null)
    {
        DrawLine(
                new PointValue(start_x),
                new PointValue(start_y),
                new PointValue(end_x),
                new PointValue(end_y),
                c,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawLine(IPoint start_x, IPoint start_y, IPoint end_x, IPoint end_y, IColor? c = null, IPoint? linewidth = null)
    {
        Operations.Add(new DrawLine()
        {
            StartX = start_x,
            StartY = start_y,
            EndX = end_x,
            EndY = end_y,
            Color = c,
            LineWidth = linewidth ?? new PointValue(1),
        });
    }

    public void DrawRectangle(double x, double y, double width, double height, IColor? c = null, double? linewidth = null)
    {
        DrawRectangle(
                new PointValue(x),
                new PointValue(y),
                new PointValue(width),
                new PointValue(height),
                c,
                new PointValue(linewidth ?? 1)
            );
    }

    public void DrawRectangle(IPoint x, IPoint y, IPoint width, IPoint height, IColor? c = null, IPoint? linewidth = null)
    {
        Operations.Add(new DrawRectangle()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Color = c,
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
