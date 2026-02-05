using PicoPDF.Pdf.Encoding;
using Xunit;

namespace PicoPDF.Test;

public class UTF16WithBOMTest
{
    [Fact]
    public void NotSupportedBETest()
    {
        var enc = UTF16WithBOM.UTF16_BEWithBOM;

        Assert.Equal<byte[]>(enc.GetBytes([]), []);
        Assert.Equal(enc.GetByteCount(""), 0);
        Assert.Equal(enc.GetByteCount("abc"), 6);
    }

    [Fact]
    public void NotSupportedLETest()
    {
        var enc = UTF16WithBOM.UTF16_LEWithBOM;

        Assert.Equal<byte[]>(enc.GetBytes([]), []);
        Assert.Equal(enc.GetByteCount(""), 0);
        Assert.Equal(enc.GetByteCount("abc"), 6);
    }

    [Fact]
    public void GetBytesBETest()
    {
        var enc = UTF16WithBOM.UTF16_BEWithBOM;

        Assert.Equal(enc.GetBytes(""), [0xFE, 0xFF]);
        Assert.Equal(enc.GetBytes("abc"), [0xFE, 0xFF, 0x00, 0x61, 0x00, 0x62, 0x00, 0x63]);
    }

    [Fact]
    public void GetBytesLETest()
    {
        var enc = UTF16WithBOM.UTF16_LEWithBOM;

        Assert.Equal(enc.GetBytes(""), [0xFF, 0xFE]);
        Assert.Equal(enc.GetBytes("abc"), [0xFF, 0xFE, 0x61, 0x00, 0x62, 0x00, 0x63, 0x00]);
    }
}
