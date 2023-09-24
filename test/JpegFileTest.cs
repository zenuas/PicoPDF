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
        Assert.Equal(300, jpeg.Width);
        Assert.Equal(150, jpeg.Height);
        Assert.Equal(8, jpeg.Precision);
    }
}
