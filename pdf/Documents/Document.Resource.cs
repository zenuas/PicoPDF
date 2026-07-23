using Pdf.ExtGState;
using Pdf.Shading;
using Pdf.XObject.Image;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Documents;

public partial class Document
{
    public List<IImageXObject> Images { get; init; } = [];
    public List<IShading> Shadings { get; init; } = [];
    public List<IGraphicsStateParameter> GraphicsStateParameters { get; init; } = [];

    public Func<string, IImageXObject> CreateImageCache()
    {
        var imagecache = Images.ToDictionary(x => x.Name, x => x);
        return (path) =>
        {
            if (imagecache.TryGetValue(path, out var value)) return value;
            var x = IImageXObject.Load($"X{imagecache.Count}", path);
            Images.Add(x);
            imagecache.Add(path, x);
            return x;
        };
    }
}
