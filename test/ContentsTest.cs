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

        var top = 2;
        var bottom = 2 + 8;
        var left = 1;
        var right = 1 + 4;

        contens.DrawBorderStyle(style, top, left, right - left, bottom - top);
        return [.. contens.Operations
                .OfType<DrawLine>()
                .Select(line => line.Points.Select(pt => ((int)pt.X.ToPoint(), (int)pt.Y.ToPoint())).ToArray())
            ];
    }

    [Fact]
    public void DrawBorderStyleTest()
    {
        var contens = new Contents() { Page = null! };

        var top = 2;
        var bottom = 2 + 8;
        var left = 1;
        var right = 1 + 4;

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
