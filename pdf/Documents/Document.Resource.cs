using Image;
using Image.Png;
using Mina.Extension;
using Pdf.ExtGState;
using Pdf.Shading;
using Pdf.XObject.Image;
using System;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public Func<string, IImageXObject> CreateImageCache()
    {
        var imagecache = PdfObjects.OfType<IImageXObject>().ToDictionary(x => x.Name, x => x);
        return (path) =>
        {
            if (imagecache.TryGetValue(path, out var value)) return value;
            var load = ImageLoader.FromFile(path)!;
            var x = AddImage($"X{imagecache.Count}", path, load.Width, load.Height);
            imagecache.Add(path, x);
            return x;
        };
    }

    public IImageXObject AddImage(string name, string path, int width, int height)
    {
        IImageXObject image = ImageLoader.TypeCheck(path) switch
        {
            ImageTypes.Jpeg => new JpegXObject() { Name = name, Width = width, Height = height, Path = path },
            ImageTypes.Png => new ImageXObject() { Name = name, Width = width, Height = height, Canvas = ImageLoader.FromFile(path, ImageTypes.Png)!.Cast<PngFile>().Canvas },
            _ => throw new(),
        };
        PdfObjects.Add(image.Cast<PdfObject>());

        return image;
    }

    public IShading AddShading(IShading shading)
    {
        PdfObjects.Add(shading.Cast<PdfObject>());

        return shading;
    }

    public IGraphicsStateParameter AddGraphicsStateParameter(IGraphicsStateParameter gstate)
    {
        PdfObjects.Add(gstate.Cast<PdfObject>());

        return gstate;
    }
}
