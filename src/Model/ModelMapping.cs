using Image;
using Mina.Extension;
using PicoPDF.Model.Elements;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static Func<string, Type0Font> CreateFontCache(Document doc)
    {
        var fontcache = doc.PdfObjects.OfType<Type0Font>().ToDictionary(x => x.Name, x => x);
        return (name) =>
        {
            if (fontcache.TryGetValue(name, out var value)) return value;
            var x = doc.AddFont($"F{fontcache.Count}", doc.FontRegister.LoadRequiredTables(name));
            fontcache.Add(name, x);
            return x;
        };
    }

    public static Func<ImageModel, IImageXObject> CreateImageCache(Document doc)
    {
        var imagecache = doc.PdfObjects.OfType<IImageXObject>().ToDictionary(x => x.Name, x => x);
        return (image) =>
        {
            if (imagecache.TryGetValue(image.Path, out var value)) return value;
            var load = ImageLoader.FromFile(image.Path)!;
            var x = doc.AddImage($"X{imagecache.Count}", image.Path, load.Width, load.Height);
            imagecache.Add(image.Path, x);
            return x;
        };
    }

    public static void Mapping(Document doc, PageModel[] pages)
    {
        var fontget = CreateFontCache(doc);
        var imageget = CreateImageCache(doc);
        foreach (var page in pages)
        {
            var pdfpage = doc.NewPage(page.Width, page.Height);
            page.Models.Each(section => section.Elements.Each(x => Mapping(pdfpage, fontget, imageget, x, section.Top, section.Left)));
        }
    }

    public static void Mapping(Page page, Func<string, Type0Font> fontget, Func<ImageModel, IImageXObject> imageget, IModelElement model, int top, int left)
    {
        double posx = model.X + left;
        double posy = model.Y + top;
        switch (model)
        {
            case ITextModel x:
                _ = page.Contents.DrawText(x.Text, posy, posx, x.Size, [.. x.Font.Select(fontget)], x.Width, x.Height, x.Style, x.Alignment, x.Color?.ToDeviceRGB());
                return;

            case ILineModel x:
                page.Contents.DrawLine(posx, posy, posx + x.Width, posy + x.Height, x.Color?.ToDeviceRGB(), x.LineWidth);
                return;

            case IRectangleModel x:
                page.Contents.DrawRectangle(posx, posy, x.Width, x.Height, x.Color?.ToDeviceRGB(), x.LineWidth);
                return;

            case IFillRectangleModel x:
                page.Contents.DrawFillRectangle(posx, posy, x.Width, x.Height, x.LineColor.ToDeviceRGB(), x.FillColor.ToDeviceRGB(), x.LineWidth);
                return;

            case ImageModel x:
                page.Contents.DrawImage(posx, posy, imageget(x), x.ZoomWidth, x.ZoomHeight);
                return;
        }
        throw new();
    }
}
