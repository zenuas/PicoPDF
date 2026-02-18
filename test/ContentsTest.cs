using PicoPDF.Binder.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Operation;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class ContentsTest
{
    public static (int X, int Y)[][] GetBorderStrokes(TextStyle style)
    {
        var contens = new Contents() { Page = null! };

        var width = 1 << 0;
        var height = 1 << 1;
        var top = 1 << 2;
        var left = 1 << 3;
        var bottom = top + height;
        var right = left + width;

        contens.DrawBorderStyle(style, top, left, width, height);
        return [.. contens.Operations
                .OfType<DrawLine>()
                .Select(line => line.Points.Select(pt => ((int)pt.X.ToPoint(), (int)pt.Y.ToPoint())).ToArray())
            ];
    }

    [Fact]
    public void DrawBorderStyleTest()
    {
        var width = 1 << 0;
        var height = 1 << 1;
        var top = 1 << 2;
        var left = 1 << 3;
        var bottom = top + height;
        var right = left + width;

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderTop), [[(left, top), (right, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderBottom), [[(left, bottom), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderLeft), [[(left, top), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderRight), [[(right, top), (right, bottom)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderTop | TextStyle.BorderBottom), [[(left, top), (right, top)], [(left, bottom), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderLeft | TextStyle.BorderRight), [[(left, top), (left, bottom)], [(right, top), (right, bottom)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderTop | TextStyle.BorderRight), [[(left, top), (right, top), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderRight | TextStyle.BorderBottom), [[(right, top), (right, bottom), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderBottom | TextStyle.BorderLeft), [[(right, bottom), (left, bottom), (left, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderLeft | TextStyle.BorderTop), [[(left, bottom), (left, top), (right, top)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderTop | TextStyle.BorderRight | TextStyle.BorderBottom), [[(left, top), (right, top), (right, bottom), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderRight | TextStyle.BorderBottom | TextStyle.BorderLeft), [[(right, top), (right, bottom), (left, bottom), (left, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderTop), [[(right, bottom), (left, bottom), (left, top), (right, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyle.BorderLeft | TextStyle.BorderTop | TextStyle.BorderRight), [[(left, bottom), (left, top), (right, top), (right, bottom)]]);
    }
}
