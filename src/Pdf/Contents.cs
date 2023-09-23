using Extensions;
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
    public List<IOperation> Operations { get; init; } = new();

    public void DrawString(string s, int x, int y, int size, IFont font, IColor? c = null)
    {
        DrawString(
                s,
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                size,
                font,
                c
            );
    }

    public void DrawString(string s, IPoint x, IPoint y, int size, IFont font, IColor? c = null)
    {
        Operations.Add(new DrawString()
        {
            Text = s,
            X = x,
            Y = y,
            FontSize = size,
            Font = font,
            Color = c,
        });
    }

    public void DrawLine(int start_x, int start_y, int end_x, int end_y, IColor? c = null)
    {
        DrawLine(
                new PointValue() { Value = start_x },
                new PointValue() { Value = start_y },
                new PointValue() { Value = end_x },
                new PointValue() { Value = end_y },
                c
            );
    }

    public void DrawLine(IPoint start_x, IPoint start_y, IPoint end_x, IPoint end_y, IColor? c = null)
    {
        Operations.Add(new DrawLine()
        {
            StartX = start_x,
            StartY = start_y,
            EndX = end_x,
            EndY = end_y,
            Color = c,
        });
    }

    public void DrawRectangle(int x, int y, int width, int height, IColor? c = null)
    {
        DrawRectangle(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                new PointValue() { Value = width },
                new PointValue() { Value = height },
                c
            );
    }

    public void DrawRectangle(IPoint x, IPoint y, IPoint width, IPoint height, IColor? c = null)
    {
        Operations.Add(new DrawRectangle()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            Color = c,
        });
    }

    public void DrawFillRectangle(int x, int y, int width, int height, IColor? line = null, IColor? fill = null)
    {
        DrawFillRectangle(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                new PointValue() { Value = width },
                new PointValue() { Value = height },
                line,
                fill
            );
    }

    public void DrawFillRectangle(IPoint x, IPoint y, IPoint width, IPoint height, IColor? line = null, IColor? fill = null)
    {
        Operations.Add(new DrawFillRectangle()
        {
            X = x,
            Y = y,
            Width = width,
            Height = height,
            LineColor = line,
            FillColor = fill,
        });
    }

    public void DrawImage(int x, int y, ImageXObject image)
    {
        DrawImage(
                new PointValue() { Value = x },
                new PointValue() { Value = y },
                image
            );
    }

    public void DrawImage(IPoint x, IPoint y, ImageXObject image)
    {
        Operations.Add(new DrawImage()
        {
            X = x,
            Y = y,
            Image = image,
        });
    }

    public override void DoExport(PdfExportOption option)
    {
        var (width, height) = PdfUtility.GetPageSize(Page.Size, Page.Orientation);
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        Operations.Each(x => x.OperationWrite(width, height, writer));
        writer.Flush();
    }
}
