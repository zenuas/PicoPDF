using Image;
using Image.Png;
using Xunit;

namespace PicoPDF.Test;

public class PngTest
{
    public static readonly string TestDirectory = "../../../../test-case/PngSuite-2017jul19/";

    [Fact]
    public void Load_basi0g01()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basi0g01.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basi0g02()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basi0g02.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basi0g04()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basi0g04.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basi0g08()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basi0g08.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basi0g16()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basi0g16.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }
}
