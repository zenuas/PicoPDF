using PicoPDF.OpenType.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PicoPDF.Test;

public class CMapFormat4Test
{
    public static (char Start, char End)[] CreateStartEnds(Dictionary<char, ushort> char_gid) => CMapFormat4.CreateStartEnds(char_gid.Keys.Order().ToArray(), char_gid);

    [Fact]
    public void CreateStartEndsTest()
    {
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0 }).AsSpan(), [('A', 'A'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1 }).AsSpan(), [('A', 'B'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2 }).AsSpan(), [('A', 'C'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3 }).AsSpan(), [('A', 'D'), ((char)0xFFFF, (char)0xFFFF)]);

        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1 }).AsSpan(), [('A', 'A'), ('X', 'X'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2 }).AsSpan(), [('A', 'B'), ('X', 'X'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3 }).AsSpan(), [('A', 'C'), ('X', 'X'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4 }).AsSpan(), [('A', 'D'), ('X', 'X'), ((char)0xFFFF, (char)0xFFFF)]);

        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1, ['Y'] = 2 }).AsSpan(), [('A', 'A'), ('X', 'Y'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2, ['Y'] = 3 }).AsSpan(), [('A', 'B'), ('X', 'Y'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3, ['Y'] = 4 }).AsSpan(), [('A', 'C'), ('X', 'Y'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4, ['Y'] = 5 }).AsSpan(), [('A', 'D'), ('X', 'Y'), ((char)0xFFFF, (char)0xFFFF)]);

        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['X'] = 1, ['Y'] = 2, ['Z'] = 3 }).AsSpan(), [('A', 'A'), ('X', 'Z'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['X'] = 2, ['Y'] = 3, ['Z'] = 4 }).AsSpan(), [('A', 'B'), ('X', 'Z'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['X'] = 3, ['Y'] = 4, ['Z'] = 5 }).AsSpan(), [('A', 'C'), ('X', 'Z'), ((char)0xFFFF, (char)0xFFFF)]);
        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 1, ['C'] = 2, ['D'] = 3, ['X'] = 4, ['Y'] = 5, ['Z'] = 6 }).AsSpan(), [('A', 'D'), ('X', 'Z'), ((char)0xFFFF, (char)0xFFFF)]);

        Assert.Equal(CreateStartEnds(new Dictionary<char, ushort>() { ['A'] = 0, ['B'] = 2 }).AsSpan(), [('A', 'A'), ('B', 'B'), ((char)0xFFFF, (char)0xFFFF)]);
    }

    [Fact]
    public void CreateStartEndsErrorTest()
    {
        _ = Assert.Throws<IndexOutOfRangeException>(() => CreateStartEnds([]));
    }
}
