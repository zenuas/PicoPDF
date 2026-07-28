using Mina.Extension;
using Pdf.ExtGState;
using Pdf.Shading;
using Pdf.XObject;
using Pdf.XObject.Image;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public List<IXObject> XObjects { get; init; } = [];
    public List<IShading> Shadings { get; init; } = [];
    public List<IGraphicsStateParameter> GraphicsStateParameters { get; init; } = [];

    public Func<string, IImageXObject> CreateImageCache()
    {
        var imagecache = XObjects.ToDictionary(x => x.Name, x => x);
        return (path) =>
        {
            if (imagecache.TryGetValue(path, out var value)) return value.Cast<IImageXObject>();
            var x = IImageXObject.Load($"X{imagecache.Count}", path);
            XObjects.Add(x);
            imagecache.Add(path, x);
            return x;
        };
    }
}
