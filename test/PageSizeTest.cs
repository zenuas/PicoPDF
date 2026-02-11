using PicoPDF.Binder.Data;
using Xunit;

namespace PicoPDF.Test;

public class PageSizeTest
{
    [Fact]
    public void TryParse()
    {
        var ok1 = PageSize.TryParse("A4", out var v1);
        Assert.Equal(ok1, true);
        Assert.Equal(v1, new PageSize(PageSizes.A4));

        var ok2 = PageSize.TryParse("A3", out var v2);
        Assert.Equal(ok2, true);
        Assert.Equal(v2, new PageSize(842, 1191));

        var ok3 = PageSize.TryParse("1191, 1684", out var v3);
        Assert.Equal(ok3, true);
        Assert.Equal(v3, new PageSize(PageSizes.A2));

        var ok4 = PageSize.TryParse("100, 200", out var v4);
        Assert.Equal(ok4, true);
        Assert.Equal(v4, new PageSize(100, 200));

        Assert.Equal(PageSize.TryParse("", out var _), false);
        Assert.Equal(PageSize.TryParse("", out var _), false);
        Assert.Equal(PageSize.TryParse(",", out var _), false);
        Assert.Equal(PageSize.TryParse("595,", out var _), false);
        Assert.Equal(PageSize.TryParse(", 842", out var _), false);
        Assert.Equal(PageSize.TryParse("595, A4", out var _), false);
        Assert.Equal(PageSize.TryParse("A4, 842", out var _), false);
        Assert.Equal(PageSize.TryParse("X99", out var _), false);
    }
}
