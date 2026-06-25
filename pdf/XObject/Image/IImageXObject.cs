using Image;
using Image.Png;
using Mina.Extension;

namespace Pdf.XObject.Image;

public interface IImageXObject : IXObject
{
    public string Name { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public static IImageXObject Load(string name, string path, int width, int height) => ImageLoader.TypeCheck(path) switch
    {
        ImageTypes.Jpeg => new JpegXObject() { Name = name, Width = width, Height = height, Path = path },
        ImageTypes.Png => new ImageXObject() { Name = name, Width = width, Height = height, Canvas = ImageLoader.FromFile(path, ImageTypes.Png)!.Cast<PngFile>().Canvas },
        _ => throw new(),
    };
}
