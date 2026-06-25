using Image;
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
            var x = IImageXObject.Load($"X{imagecache.Count}", path, load.Width, load.Height);
            AddImage(x);
            imagecache.Add(path, x);
            return x;
        };
    }

    public void AddImage(IImageXObject image) => PdfObjects.Add(image.Cast<PdfObject>());

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
