using PicoPDF.Binder;
using System;
using System.Globalization;
using Xunit;

namespace PicoPDF.Test;

public class BindSummaryMapperTest
{
    [Fact]
    public void BindFormatInvariantCultureTest()
    {
        var culture = CultureInfo.InvariantCulture;

        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "", culture), "");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "#,0", culture), "");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(1, "#,0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(999, "#,0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_000, "#,0", culture), "1,000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567, "#,0", culture), "1,234,567");

        Assert.Equal(BindSummaryMapper<int>.BindFormat("1", "#,0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("999", "#,0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1000", "#,0", culture), "1000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567", "#,0", culture), "1234567");

        var d = DateTime.Parse("4321-12-31T12:34:56", CultureInfo.InvariantCulture);
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "d", culture), "12/31/4321");
    }
}
