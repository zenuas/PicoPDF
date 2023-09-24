using PicoPDF.Image;
using Xunit;

namespace PicoPDF.Test;

public class ImageLoaderTest
{
    [Fact]
    public void TypeMatch1()
    {
        Assert.Equal(ImageTypes.Unknown, ImageLoader.TypeCheck("../../../../test-case/01.json"));
    }

    [Fact]
    public void TypeMatch2()
    {
        Assert.Equal(ImageTypes.Jpeg, ImageLoader.TypeCheck("../../../../test-case/300x150.jpg"));
    }

    [Fact]
    public void TypeMatchError1()
    {
        _ = Assert.Throws<DirectoryNotFoundException>(() => ImageLoader.TypeCheck("../../../../notfound/01.json"));
    }

    [Fact]
    public void TypeMatchError2()
    {
        _ = Assert.Throws<FileNotFoundException>(() => ImageLoader.TypeCheck("../../../../test-case/notfound.json"));
    }
}
