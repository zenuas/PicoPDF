using Image;
using Image.Bmp;
using Image.Png;
using System.IO;
using Xunit;

namespace PicoPDF.Test;

public class PngTest
{
    public static readonly string TestDirectory = "../../../../test-case/PngSuite-2017jul19/";

    public static void WriteBitmap(PngFile png, string path)
    {
        var bmp = new BmpFile
        {
            Width = png.Width,
            Height = png.Height,
            Canvas = png.Canvas,
        };
        using var file = File.Create(path);
        bmp.Write(file);
    }

    // Basic formats
    [Fact]
    public void Load_basn0g01()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basn0g01.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basn0g02()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basn0g02.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basn0g04()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basn0g04.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basn0g08()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basn0g08.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }

    [Fact]
    public void Load_basn0g16()
    {
        var image = ImageLoader.FromFile($"{TestDirectory}/basn0g16.png");
        var png = Assert.IsType<PngFile>(image);
        Assert.Equal(png.Width, 32);
        Assert.Equal(png.Height, 32);
        Assert.Equal(png.Canvas.Length, 32);
    }
}
