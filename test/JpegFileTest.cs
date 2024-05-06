using PicoPDF.Image;
using PicoPDF.Image.Jpeg;
using Xunit;

namespace PicoPDF.Test;

public class JpegFileTest
{
    [Fact]
    public void GetSize1()
    {
        var image = ImageLoader.FromFile("../../../../test-case/300x150.jpg", ImageTypes.Jpeg);
        var jpeg = Assert.IsType<JpegFile>(image);
        Assert.Equal(jpeg.Width, 300);
        Assert.Equal(jpeg.Height, 150);
        Assert.Equal(jpeg.Precision, 8);
    }
}
