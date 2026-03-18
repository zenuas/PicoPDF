using OpenType.Tables.CMap;
using System;
using Xunit;

namespace PicoPDF.Test;

public class CMapFormat12Test
{
    [Fact]
    public void CreateStartEndsTest()
    {
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0)]).AsSpan(),
            [
                ('A', 'A', 0U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1)]).AsSpan(),
            [
                ('A', 'B', 0U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2)]).AsSpan(),
            [
                ('A', 'C', 0U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3)]).AsSpan(),
            [
                ('A', 'D', 0U),
            ]);

        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('X', 1)]).AsSpan(),
            [
                ('A', 'A', 0U),
                ('X', 'X', 1U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('X', 2)]).AsSpan(),
            [
                ('A', 'B', 0U),
                ('X', 'X', 2U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3)]).AsSpan(),
            [
                ('A', 'C', 0U),
                ('X', 'X', 3U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4)]).AsSpan(),
            [
                ('A', 'D', 0U),
                ('X', 'X', 4U),
            ]);

        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('X', 1), ('Y', 2)]).AsSpan(),
            [
                ('A', 'A', 0U),
                ('X', 'Y', 1U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('X', 2), ('Y', 3)]).AsSpan(),
            [
                ('A', 'B', 0U),
                ('X', 'Y', 2U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3), ('Y', 4)]).AsSpan(),
            [
                ('A', 'C', 0U),
                ('X', 'Y', 3U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4), ('Y', 5)]).AsSpan(),
            [
                ('A', 'D', 0U),
                ('X', 'Y', 4U),
            ]);

        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('X', 1), ('Y', 2), ('Z', 3)]).AsSpan(),
            [
                ('A', 'A', 0U),
                ('X', 'Z', 1U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('X', 2), ('Y', 3), ('Z', 4)]).AsSpan(),
            [
                ('A', 'B', 0U),
                ('X', 'Z', 2U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3), ('Y', 4), ('Z', 5)]).AsSpan(),
            [
                ('A', 'C', 0U),
                ('X', 'Z', 3U),
            ]);
        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4), ('Y', 5), ('Z', 6)]).AsSpan(),
            [
                ('A', 'D', 0U),
                ('X', 'Z', 4U),
            ]);

        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 2)]).AsSpan(),
            [
                ('A', 'A', 0U),
                ('B', 'B', 2U),
            ]);

        Assert.Equal(CMapFormat12.CreateStartEnds([('A', 0), ('B', 0)]).AsSpan(),
            [
                ('A', 'A', 0U),
                ('B', 'B', 0U),
            ]);
    }

    [Fact]
    public void CreateStartEndsErrorTest()
    {
        (uint Start, uint End, uint GID)[] nodata = [];
        Assert.Equal(CMapFormat12.CreateStartEnds([]), nodata);
    }
}
