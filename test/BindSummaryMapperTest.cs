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
        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "N", culture), "");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(1, "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(999, "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_000, "N0", culture), "1,000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567, "N0", culture), "1,234,567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567.89, "N2", culture), "1,234,567.89");

        Assert.Equal(BindSummaryMapper<int>.BindFormat("1", "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("999", "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1000", "N0", culture), "1000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567", "N0", culture), "1234567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567.89", "N2", culture), "1234567.89");

        var d = DateTime.Parse("4321-12-31T12:34:56", CultureInfo.InvariantCulture);
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "d", culture), "12/31/4321");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "D", culture), "Saturday, 31 December 4321");
    }

    [Fact]
    public void BindFormatJapaneseCultureTest()
    {
        var culture = CultureInfo.GetCultureInfo("ja-JP");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "", culture), "");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "N", culture), "");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(1, "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(999, "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_000, "N0", culture), "1,000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567, "N0", culture), "1,234,567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567.89, "N2", culture), "1,234,567.89");

        Assert.Equal(BindSummaryMapper<int>.BindFormat("1", "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("999", "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1000", "N0", culture), "1000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567", "N0", culture), "1234567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567.89", "N2", culture), "1234567.89");

        var d = DateTime.Parse("4321-12-31T12:34:56", CultureInfo.InvariantCulture);
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "d", culture), "4321/12/31");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "D", culture), "4321年12月31日土曜日");
    }

    [Fact]
    public void BindFormaFranceCultureTest()
    {
        var culture = CultureInfo.GetCultureInfo("fr-FR");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "", culture), "");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(null, "N", culture), "");

        Assert.Equal(BindSummaryMapper<int>.BindFormat(1, "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(999, "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_000, "N0", culture), "1 000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567, "N0", culture), "1 234 567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(1_234_567.89, "N2", culture), "1 234 567,89");

        Assert.Equal(BindSummaryMapper<int>.BindFormat("1", "N0", culture), "1");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("999", "N0", culture), "999");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1000", "N0", culture), "1000");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567", "N0", culture), "1234567");
        Assert.Equal(BindSummaryMapper<int>.BindFormat("1234567.89", "N2", culture), "1234567.89");

        var d = DateTime.Parse("4321-12-31T12:34:56", CultureInfo.InvariantCulture);
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "d", culture), "31/12/4321");
        Assert.Equal(BindSummaryMapper<int>.BindFormat(d, "D", culture), "samedi 31 décembre 4321");
    }
}
