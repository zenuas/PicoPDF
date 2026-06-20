using Pdf.Documents;
using Pdf.Operation;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class ContentsTest
{
    public static (int X, int Y)[][] GetBorderStrokes(TextStyles style)
    {
        var left = 1;
        var top = 2;
        var height = 4;
        var width = 8;
        var bottom = top + height;
        var right = left + width;

        return [.. DrawOperations.CreateBorderStyle(style, top, left, width, height)
                .OfType<DrawLine>()
                .Select(line => line.Points.Select(pt => ((int)pt.X.ToPoint(), (int)pt.Y.ToPoint())).ToArray())
            ];
    }

    [Fact]
    public void DrawBorderStyleTest()
    {
        /* all coordinate values ​​are unique.
           
           topleft(x=1, y=2)               topright(x=9, y=2)
            +------------------------------------------+
            |                  width=8                 |
            | height=4                                 |
            |                                          |
            +------------------------------------------+
           bottomleft(x=1, y=6)         bottomright(x=9, y=6)
         */
        var left = 1;
        var top = 2;
        var height = 4;
        var width = 8;
        var bottom = top + height;
        var right = left + width;

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderTop), [[(left, top), (right, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderBottom), [[(left, bottom), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderLeft), [[(left, top), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderRight), [[(right, top), (right, bottom)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderTop | TextStyles.BorderBottom), [[(left, top), (right, top)], [(left, bottom), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderLeft | TextStyles.BorderRight), [[(left, top), (left, bottom)], [(right, top), (right, bottom)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderTop | TextStyles.BorderRight), [[(left, top), (right, top), (right, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderRight | TextStyles.BorderBottom), [[(right, top), (right, bottom), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderBottom | TextStyles.BorderLeft), [[(right, bottom), (left, bottom), (left, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderLeft | TextStyles.BorderTop), [[(left, bottom), (left, top), (right, top)]]);

        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderTop | TextStyles.BorderRight | TextStyles.BorderBottom), [[(left, top), (right, top), (right, bottom), (left, bottom)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderRight | TextStyles.BorderBottom | TextStyles.BorderLeft), [[(right, top), (right, bottom), (left, bottom), (left, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderBottom | TextStyles.BorderLeft | TextStyles.BorderTop), [[(right, bottom), (left, bottom), (left, top), (right, top)]]);
        Assert.Equal<(int, int)[][]>(GetBorderStrokes(TextStyles.BorderLeft | TextStyles.BorderTop | TextStyles.BorderRight), [[(left, bottom), (left, top), (right, top), (right, bottom)]]);
    }
}
