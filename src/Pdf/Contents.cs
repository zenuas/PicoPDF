using Mina.Extension;
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
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                size,
                font,
                c,
                clip
            );
    }

    public void DrawString(string s, IPoint x, IPoint y, double size, IFont font, IColor? c = null, Rectangle? clip = null)
    {
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
            Operations.Add(new DrawCliping()
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

    public void DrawLine(double start_x, double start_y, double end_x, double end_y, IColor? c = null, double? linewidth = null)
    {
        DrawLine(
                new PointValue() { Value = start_x },
                new PointValue() { Value = start_y },
                new PointValue() { Value = end_x },
                new PointValue() { Value = end_y },
                c,
                new PointValue() { Value = linewidth ?? 1 }
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
            LineWidth = linewidth ?? new PointValue() { Value = 1 },
        });
    }

    public void DrawRectangle(double x, double y, double width, double height, IColor? c = null, double? linewidth = null)
    {
        DrawRectangle(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                new PointValue() { Value = width },
                new PointValue() { Value = height },
                c,
                new PointValue() { Value = linewidth ?? 1 }
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
            LineWidth = linewidth ?? new PointValue() { Value = 1 },
        });
    }

    public void DrawFillRectangle(double x, double y, double width, double height, IColor? line = null, IColor? fill = null, double? linewidth = null)
    {
        DrawFillRectangle(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                new PointValue() { Value = width },
                new PointValue() { Value = height },
                line,
                fill,
                new PointValue() { Value = linewidth ?? 1 }
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
            LineWidth = linewidth ?? new PointValue() { Value = 1 },
        });
    }

    public void DrawImage(double x, double y, IImageXObject image, double zoomwidth = 1.0, double zoomheight = 1.0)
    {
        DrawImage(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
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
        var (width, height) = PdfUtility.GetPageSize(Page.Size, Page.Orientation);
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        Operations.Each(x => x.OperationWrite(width, height, writer, option));
        writer.Flush();
    }
}
