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
            case ITextModel x:
                {
                    var font = fontget(x.Font[0]);
                    var box = font.MeasureStringBox(x.Text);
                    var size = x.Style.HasFlag(TextStyle.ShrinkToFit) && x.Width < (box.Width * x.Size) ? x.Width / box.Width : x.Size;
                    posy += -box.Top * size;
                    switch (x.Alignment)
                    {
                        case TextAlignment.Center:
                            posx += (x.Width - (box.Width * size)) / 2;
                            break;

                        case TextAlignment.End:
                            posx += x.Width - (box.Width * size);
                            break;
                    }
                    var rect = !x.Style.HasFlag(TextStyle.Clipping) ? (Rectangle?)null : new Rectangle()
                    {
                        X = new PointValue(model.X + left),
                        Y = new PointValue(model.Y + top),
                        Width = new PointValue(x.Width),
                        Height = new PointValue(box.Height * size),
                    };
                    page.Contents.DrawString(x.Text, posx, posy, size, font, x.Color?.ToDeviceRGB(), rect);

                    if (x.Style != TextStyle.None)
                    {
                        var width = (int)(box.Width * size);
                        var topleft = model.Y + top;
                        var bottomleft = (int)(topleft + (box.Height * size));

                        if (x.Style.HasFlag(TextStyle.Underline))
                        {
                            page.Contents.DrawLine(posx, posy, posx + width, posy, x.Color?.ToDeviceRGB());
                        }
                        if (x.Style.HasFlag(TextStyle.DoubleUnderline))
                        {
                            page.Contents.DrawLine(posx, posy + 2, posx + width, posy + 2, x.Color?.ToDeviceRGB());
                        }
                        if (x.Style.HasFlag(TextStyle.Strikethrough))
                        {
                            var center = (int)(topleft + (box.Height * size / 2));
                            page.Contents.DrawLine(posx, center, posx + width, center, x.Color?.ToDeviceRGB());
                        }
                        if (x.Style.HasFlag(TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight))
                        {
                            page.Contents.DrawRectangle(posx, topleft, width, bottomleft - topleft, x.Color?.ToDeviceRGB());
                        }
                        else if ((x.Style & (TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight)) > 0)
                        {
                            if (x.Style.HasFlag(TextStyle.BorderTop))
                            {
                                page.Contents.DrawLine(posx, topleft, posx + width, topleft, x.Color?.ToDeviceRGB());
                            }
                            if (x.Style.HasFlag(TextStyle.BorderBottom))
                            {
                                page.Contents.DrawLine(posx, bottomleft, posx + width, bottomleft, x.Color?.ToDeviceRGB());
                            }
                            if (x.Style.HasFlag(TextStyle.BorderLeft))
                            {
                                page.Contents.DrawLine(posx, topleft, posx, bottomleft, x.Color?.ToDeviceRGB());
                            }
                            if (x.Style.HasFlag(TextStyle.BorderRight))
                            {
                                page.Contents.DrawLine(posx + width, topleft, posx + width, bottomleft, x.Color?.ToDeviceRGB());
                            }
                        }
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
