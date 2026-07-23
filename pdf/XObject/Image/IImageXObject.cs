using Image;
using Image.Jpeg;
using Mina.Extension;
using System;
using System.Linq;

namespace Pdf.XObject.Image;

public interface IImageXObject : IXObject
{
    public string Name { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }

    public static IImageXObject Load(string name, string path)
    {
        var load = ImageLoader.FromFile(path).Try();
        return load.GetType() switch
        {
            Type a when a == typeof(JpegImage) => new JpegXObject() { Name = name, Width = load.Width, Height = load.Height, Path = path },
            _ => new ImageXObject()
            {
                Name = name,
                Width = load.Width,
                Height = load.Height,
                Canvas = [.. load
                    .Cast<IImageCanvas>()
                    .Canvas
                    .Select(x => x.Select(color => color.ToColor()).ToArray())],
            },
        };
    }
}
