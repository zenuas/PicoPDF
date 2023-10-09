using Extensions;
using PicoPDF.Binder.Element;
using PicoPDF.Image;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document doc, PageModel[] pages)
    {
        var fontcache = doc.PdfObjects.OfType<TrueTypeFont>().ToDictionary(x => x.Name, x => x);
        TrueTypeFont fontget(string name)
        {
            if (fontcache.ContainsKey(name)) return fontcache[name];
            var x = doc.AddFont($"F{fontcache.Count}", doc.FontRegister.GetOrNull(name)!);
            fontcache.Add(name, x);
            return x;
        }

        var imagecache = doc.PdfObjects.OfType<IImageXObject>().ToDictionary(x => x.Name, x => x);
        IImageXObject imageget(ImageModel image)
        {
            if (imagecache.ContainsKey(image.Path)) return imagecache[image.Path];
            var load = ImageLoader.FromFile(image.Path)!;
            var x = doc.AddImage($"X{imagecache.Count}", image.Path, load.Width, load.Height);
            imagecache.Add(image.Path, x);
            return x;
        }
        pages.Each(x => Mapping(doc.NewPage(x.Size, x.Orientation), fontget, imageget, x.Models));
    }

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, IImageXObject> imageget, List<SectionModel> models) => models.Each(x => Mapping(page, fontget, imageget, x, x.Top));

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, IImageXObject> imageget, SectionModel model, int top) => model.Elements.Each(x => Mapping(page, fontget, imageget, x, top));

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, IImageXObject> imageget, IModelElement model, int top)
    {
        var posx = model.X;
        var posy = model.Y + top;
        switch (model)
        {
            case TextModel x:
                {
                    var ttf = fontget(x.Font);
                    var box = ttf.MeasureStringBox(x.Text);
                    var size = x.Style.HasFlag(TextStyle.ShrinkToFit) && x.Width < (box.Width * x.Size) ? (int)(x.Width / box.Width) : x.Size;
                    posy += (int)(-box.Top * size);
                    switch (x.Alignment)
                    {
                        case TextAlignment.Center:
                            posx += (int)((x.Width - (box.Width * size)) / 2);
                            break;

                        case TextAlignment.End:
                            posx += (int)(x.Width - (box.Width * size));
                            break;
                    }
                    if (x.Cliping)
                    {
                        var rect = new Rectangle()
                        {
                            X = new PointValue() { Value = model.X },
                            Y = new PointValue() { Value = model.Y + top },
                            Width = new PointValue() { Value = x.Width },
                            Height = new PointValue() { Value = box.Height * size },
                        };
                        page.Contents.DrawString(x.Text, posx, posy, size, ttf, x.Color, rect);
                    }
                    else
                    {
                        page.Contents.DrawString(x.Text, posx, posy, size, ttf, x.Color);
                    }

                    if (x.Style != TextStyle.None)
                    {
                        var width = (int)(box.Width * size);
                        var topleft = model.Y + top;
                        var bottomleft = (int)(model.Y + top + (box.Height * size));

                        if (x.Style.HasFlag(TextStyle.UnderLine))
                        {
                            page.Contents.DrawLine(posx, posy, posx + width, posy, x.Color);
                        }
                        if (x.Style.HasFlag(TextStyle.UnderLine2))
                        {
                            page.Contents.DrawLine(posx, posy + 2, posx + width, posy + 2, x.Color);
                        }
                        if (x.Style.HasFlag(TextStyle.LineThrough))
                        {
                            var center = (int)(topleft + (box.Height * size / 2));
                            page.Contents.DrawLine(posx, center, posx + width, center, x.Color);
                        }
                        if (x.Style.HasFlag(TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight))
                        {
                            page.Contents.DrawRectangle(posx, topleft, width, bottomleft - topleft, x.Color);
                        }
                        else if ((x.Style & (TextStyle.BorderTop | TextStyle.BorderBottom | TextStyle.BorderLeft | TextStyle.BorderRight)) > 0)
                        {
                            if (x.Style.HasFlag(TextStyle.BorderTop))
                            {
                                page.Contents.DrawLine(posx, topleft, posx + width, topleft, x.Color);
                            }
                            if (x.Style.HasFlag(TextStyle.BorderBottom))
                            {
                                page.Contents.DrawLine(posx, bottomleft, posx + width, bottomleft, x.Color);
                            }
                            if (x.Style.HasFlag(TextStyle.BorderLeft))
                            {
                                page.Contents.DrawLine(posx, topleft, posx, bottomleft, x.Color);
                            }
                            if (x.Style.HasFlag(TextStyle.BorderRight))
                            {
                                page.Contents.DrawLine(posx + width, topleft, posx + width, bottomleft, x.Color);
                            }
                        }
                    }
                }
                return;

            case LineModel x:
                page.Contents.DrawLine(posx, posy, posx + x.Width, posy + x.Height, x.Color, x.LineWidth);
                return;

            case RectangleModel x:
                page.Contents.DrawRectangle(posx, posy, x.Width, x.Height, x.Color, x.LineWidth);
                return;

            case FillRectangleModel x:
                page.Contents.DrawFillRectangle(posx, posy, x.Width, x.Height, x.LineColor, x.FillColor, x.LineWidth);
                return;

            case ImageModel x:
                page.Contents.DrawImage(posx, posy, imageget(x), x.ZoomWidth, x.ZoomHeight);
                return;
        }
        throw new();
    }
}
