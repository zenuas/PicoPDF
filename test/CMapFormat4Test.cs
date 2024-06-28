using PicoPDF.OpenType;
using System;
using System.Collections.Generic;
using Xunit;

namespace PicoPDF.Test;

public class CMapFormat4Test
{
    [Fact]
    public void CreateStartEndsTest()
    {
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0 }), [('A', 'A')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1 }), [('A', 'B')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2 }), [('A', 'C')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3 }), [('A', 'D')]);

        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1 }), [('A', 'A'), ('X', 'X')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2 }), [('A', 'B'), ('X', 'X')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3 }), [('A', 'C'), ('X', 'X')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4 }), [('A', 'D'), ('X', 'X')]);

        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1, ['Y'] = 2 }), [('A', 'A'), ('X', 'Y')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2, ['Y'] = 3 }), [('A', 'B'), ('X', 'Y')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3, ['Y'] = 4 }), [('A', 'C'), ('X', 'Y')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4, ['Y'] = 5 }), [('A', 'D'), ('X', 'Y')]);

        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1, ['Y'] = 2, ['Z'] = 3 }), [('A', 'A'), ('X', 'Z')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2, ['Y'] = 3, ['Z'] = 4 }), [('A', 'B'), ('X', 'Z')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3, ['Y'] = 4, ['Z'] = 5 }), [('A', 'C'), ('X', 'Z')]);
        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4, ['Y'] = 5, ['Z'] = 6 }), [('A', 'D'), ('X', 'Z')]);

        Assert.Equal(CMapFormat4.CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 2 }), [('A', 'A'), ('B', 'B')]);
    }

    [Fact]
    public void CreateStartEndsErrorTest()
    {
        Assert.Throws<IndexOutOfRangeException>(() => CMapFormat4.CreateStartEnds([]));
    }
}
