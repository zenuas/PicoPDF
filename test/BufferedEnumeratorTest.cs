using PicoPDF.Section;
using Xunit;

namespace PicoPDF.Test;

public class BufferedEnumeratorTest
{
    [Fact]
    public void Test1()
    {
        var orig = new List<int> { 1, 2, 3 };
        var xs = new BufferedEnumerator<int>() { BaseEnumerator = orig.GetEnumerator() };

        Assert.True(xs.Next(0, out var i0) && i0 == 1);
        Assert.True(xs.Next(1, out var i1) && i1 == 2);
        Assert.True(xs.Next(2, out var i2) && i2 == 3);
        Assert.False(xs.Next(3, out _));

        Assert.Equal(xs.GetRange(1), [1]);
        Assert.True(xs.Next(0, out var i6) && i6 == 2);
        Assert.True(xs.Next(1, out var i7) && i7 == 3);
        Assert.False(xs.Next(2, out _));

        Assert.Equal(xs.GetRange(1), [2]);
        Assert.True(xs.Next(0, out var i8) && i8 == 3);
        Assert.False(xs.Next(1, out _));

        Assert.Equal(xs.GetRange(1), [3]);
        Assert.False(xs.Next(0, out _));

        Assert.Throws<IndexOutOfRangeException>(() => xs.GetRange(1));
    }
}
