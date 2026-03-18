using OpenType.Tables.CMap;
using System;
using Xunit;

namespace PicoPDF.Test;

public class CMapFormat4Test
{
    [Fact]
    public void CreateStartEndsTest()
    {
        Assert.Equal(CMapFormat4.CreateStartEnds(['A'], [0]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B'], [0, 1]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C'], [0, 1, 2]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'D'], [0, 1, 2, 3]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'X'], [0, 1]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'X', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'X'], [0, 1, 2]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'X', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'X'], [0, 1, 2, 3]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'X', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'D', 'X'], [0, 1, 2, 3, 4]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'X', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'X', 'Y'], [0, 1, 2]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'Y', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'X', 'Y'], [0, 1, 2, 3]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'Y', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'X', 'Y'], [0, 1, 2, 3, 4]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'Y', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'D', 'X', 'Y'], [0, 1, 2, 3, 4, 5]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'Y', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'X', 'Y', 'Z'], [0, 1, 2, 3]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'Z', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'X', 'Y', 'Z'], [0, 1, 2, 3, 4]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'Z', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'X', 'Y', 'Z'], [0, 1, 2, 3, 4, 5]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'Z', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B', 'C', 'D', 'X', 'Y', 'Z'], [0, 1, 2, 3, 4, 5, 6]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'Z', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds(['A', 'B'], [0, 2]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('B', 'B', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
    }

    [Fact]
    public void CreateStartEndsErrorTest()
    {
        (char Start, char End, ushort GID)[] nodata = [];
        Assert.Equal(CMapFormat4.CreateStartEnds([], []), nodata);
    }
}
