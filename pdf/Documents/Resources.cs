using Mina.Extension;
using Pdf.ExtGState;
using Pdf.Font;
using Pdf.Shading;
using Pdf.XObject;
using Pdf.XObject.Image;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Pdf.Documents;

public class Resources : IHaveReferences
{
    public required IFontRegister FontRegister { get; init; }
    public Func<string, FontLoadOptions, Type0Font> GetFont { get; init; }
    public Func<string, IImageXObject> GetImage { get; init; }
    public List<IFont> Fonts { get; init; } = [];
    public List<IXObject> XObjects { get; init; } = [];
    public List<IShading> Shadings { get; init; } = [];
    public List<IGraphicsStateParameter> GraphicsStateParameters { get; init; } = [];

    public Resources()
    {
        GetFont = CreateFontCache();
        GetImage = CreateImageCache();
    }

    public IEnumerable<IHaveReferences> GetReferences()
    {
        foreach (var x in Fonts.OfType<IPdfObject>()) yield return x;
        foreach (var x in XObjects.OfType<IPdfObject>()) yield return x;
        foreach (var x in Shadings.OfType<IPdfObject>()) yield return x;
        foreach (var x in GraphicsStateParameters.OfType<IPdfObject>()) yield return x;
    }

    public Func<string, FontLoadOptions, Type0Font> CreateFontCache()
    {
        var fontcache = Fonts.OfType<Type0Font>().ToDictionary(x => x.Name, x => x);
        var count = 0;
        return (name, option) =>
        {
            var vert = option.HasBit(FontLoadOptions.VerticalMask);
            var palt = option.HasFlag(FontLoadOptions.Proportional);
            var namekey = $"{name};vert={vert};palt={palt}";
            if (fontcache.TryGetValue(namekey, out var value)) return value;
            var opt = new OpenType.LoadOption
            {
                UseVertical = vert,
                UseProportional = palt,
            };
            var font = FontRegister.LoadFont(name, opt);
            var namekey2 = $"{font.Path.GetPath()};vert={vert};palt={palt}";
            if (fontcache.TryGetValue(namekey2, out var value2)) return value2;
            var x = Type0Font.Create($"F{count++}", font, option);
            Fonts.Add(x);
            fontcache.Add(namekey2, x);
            fontcache.Add(namekey, x);
            return x;
        };
    }

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
