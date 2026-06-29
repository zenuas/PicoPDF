using Pdf.Documents;
using System;
using Xunit;

namespace PicoPDF.Test;

public class XmpMetadataTest
{
    [Fact]
    public void Escape()
    {
        Assert.Equal(XmpMetadata.Escape(""), "");
        Assert.Equal(XmpMetadata.Escape("a"), "a");
        Assert.Equal(XmpMetadata.Escape("aa"), "aa");
        Assert.Equal(XmpMetadata.Escape("test <xmp element=\"value&text\\data\">"), "test &lt;xmp element=&quot;value&amp;text&#92;data&quot;&gt;");
    }

    [Fact]
    public void Format()
    {
        Assert.Equal(XmpMetadata.Format(new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc)), "2000-01-02T03:04:05Z");
        Assert.Matches(@"^2000-01-02T03:04:05T[+-]\d\d:\d\d$", XmpMetadata.Format(new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Local)));
    }
}
