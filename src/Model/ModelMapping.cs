using Mina.Extension;
using PicoPDF.Binder.Element;
using PicoPDF.Image;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Drawing;
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
            case ITextModel textmodel:
                {
                    var lines = PdfUtility.GetMultilineTextFont(textmodel.Text, [.. textmodel.Font.Select(fontget)]).ToArray();
                    var linetop = posy;
                    foreach (var textfonts in lines)
                    {
                        var allbox = PdfUtility.MeasureTextFontBox(textfonts);
                        var size = textmodel.Style.HasFlag(TextStyle.ShrinkToFit) && textmodel.Width < (allbox.Width * textmodel.Size) ? textmodel.Width / allbox.Width : textmodel.Size;
                        var width = allbox.Width * size;
                        var height = allbox.Height * size;
                        var basey = linetop - (allbox.Ascender * size);
                        var text_left = textmodel.Alignment switch
                        {
                            TextAlignment.Center => posx + ((textmodel.Width - width) / 2),
                            TextAlignment.End => posx + textmodel.Width - width,
                            _ => posx,
                        };
                        var rect = !textmodel.Style.HasFlag(TextStyle.Clipping) ? (Rectangle?)null : new Rectangle()
                        {
                            X = new PointValue(posx),
                            Y = new PointValue(linetop),
                            Width = new PointValue(textmodel.Width),
                            Height = new PointValue(height),
                        };
                        var color = textmodel.Color?.ToDeviceRGB();

                        page.Contents.DrawTextFont(textfonts, text_left, basey, size, color, rect);
                        if ((textmodel.Style & TextStyle.TextStyleMask) > 0) page.Contents.DrawTextStyle(textmodel.Style, linetop, text_left, basey, width, height, color);
                        if ((textmodel.Style & TextStyle.BorderStyleMask) > 0) page.Contents.DrawBorderStyle(textmodel.Style, linetop, posx, textmodel.Width > 0 ? textmodel.Width : width, height, color);
                        linetop += height + (allbox.LineGap * size);
                    }
                }
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
