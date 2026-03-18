using OpenType.Tables.CMap;
using System;
using Xunit;

namespace PicoPDF.Test;

public class CMapFormat4Test
{
    [Fact]
    public void CreateStartEndsTest()
    {
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0)]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1)]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2)]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3)]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('X', 1)]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'X', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('X', 2)]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'X', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3)]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'X', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4)]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'X', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('X', 1), ('Y', 2)]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'Y', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('X', 2), ('Y', 3)]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'Y', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3), ('Y', 4)]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'Y', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4), ('Y', 5)]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'Y', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('X', 1), ('Y', 2), ('Z', 3)]).AsSpan(),
            [
                ('A', 'A', (ushort)0),
                ('X', 'Z', (ushort)1),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('X', 2), ('Y', 3), ('Z', 4)]).AsSpan(),
            [
                ('A', 'B', (ushort)0),
                ('X', 'Z', (ushort)2),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('X', 3), ('Y', 4), ('Z', 5)]).AsSpan(),
            [
                ('A', 'C', (ushort)0),
                ('X', 'Z', (ushort)3),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);
        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 1), ('C', 2), ('D', 3), ('X', 4), ('Y', 5), ('Z', 6)]).AsSpan(),
            [
                ('A', 'D', (ushort)0),
                ('X', 'Z', (ushort)4),
                ((char)0xFFFF, (char)0xFFFF, (ushort)0),
            ]);

        Assert.Equal(CMapFormat4.CreateStartEnds([('A', 0), ('B', 2)]).AsSpan(),
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
        Assert.Equal(CMapFormat4.CreateStartEnds([]), nodata);
    }
}
