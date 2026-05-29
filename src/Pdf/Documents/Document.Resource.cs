using Image;
using Image.Png;
using Mina.Extension;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf.ExtGState;
using PicoPDF.Pdf.Shading;
using PicoPDF.Pdf.XObject.Image;
using System;
using System.Linq;

namespace PicoPDF.Pdf.Documents;

public partial class Document
{
    public Func<ImageModel, IImageXObject> CreateImageCache()
    {
        var imagecache = PdfObjects.OfType<IImageXObject>().ToDictionary(x => x.Name, x => x);
        return (image) =>
        {
            if (imagecache.TryGetValue(image.Path, out var value)) return value;
            var load = ImageLoader.FromFile(image.Path)!;
            var x = AddImage($"X{imagecache.Count}", image.Path, load.Width, load.Height);
            imagecache.Add(image.Path, x);
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
