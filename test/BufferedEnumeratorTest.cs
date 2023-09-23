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
        Assert.False(xs.Next(3, out var _));

        Assert.True(xs.MoveNext());
        Assert.Equal(1, xs.Current);
        Assert.True(xs.Next(0, out var i3) && i3 == 1);
        Assert.True(xs.Next(1, out var i4) && i4 == 2);
        Assert.True(xs.Next(2, out var i5) && i5 == 3);
        Assert.False(xs.Next(3, out var _));

        Assert.True(xs.MoveNext());
        Assert.Equal(2, xs.Current);
        Assert.True(xs.Next(0, out var i6) && i6 == 2);
        Assert.True(xs.Next(1, out var i7) && i7 == 3);
        Assert.False(xs.Next(3, out var _));

        Assert.True(xs.MoveNext());
        Assert.Equal(3, xs.Current);
        Assert.True(xs.Next(0, out var i8) && i8 == 3);
        Assert.False(xs.Next(3, out var _));

        Assert.False(xs.MoveNext());
        Assert.False(xs.Next(3, out var _));
    }
}
