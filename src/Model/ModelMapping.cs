﻿using Extensions;
using PicoPDF.Binder.Element;
using PicoPDF.Image;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
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

        var imagecache = doc.PdfObjects.OfType<ImageXObject>().ToDictionary(x => x.Name, x => x);
        ImageXObject imageget(ImageModel image)
        {
            if (imagecache.ContainsKey(image.Path)) return imagecache[image.Path];
            var load = ImageLoader.FromFile(image.Path)!;
            var x = doc.AddImage($"X{imagecache.Count}", image.Path, load.Width, load.Height);
            imagecache.Add(image.Path, x);
            return x;
        }
        pages.Each(x => Mapping(doc.NewPage(x.Size, x.Orientation), fontget, imageget, x.Models));
    }

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, ImageXObject> imageget, List<SectionModel> models)
    {
        models.Each(x => Mapping(page, fontget, imageget, x, x.Top));
    }

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, ImageXObject> imageget, SectionModel model, int top) => model.Elements.Each(x => Mapping(page, fontget, imageget, x, top));

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, Func<ImageModel, ImageXObject> imageget, IModelElement model, int top)
    {
        var posx = model.X;
        var posy = model.Y + top;
        switch (model)
        {
            case TextModel x:
                {
                    var ttf = fontget(x.Font);
                    switch (x.Alignment)
                    {
                        case TextAlignment.Start:
                            page.Contents.DrawString(x.Text, posx, posy, x.Size, ttf);
                            break;

                        case TextAlignment.Center:
                            {
                                var width = ttf.MeasureStringBox(x.Text).Width * x.Size;
                                page.Contents.DrawString(x.Text, (int)(posx + ((x.Width - width) / 2)), posy, x.Size, ttf);
                            }
                            break;

                        case TextAlignment.End:
                            {
                                var width = ttf.MeasureStringBox(x.Text).Width * x.Size;
                                page.Contents.DrawString(x.Text, (int)(posx + x.Width - width), posy, x.Size, ttf);
                            }
                            break;
                    }
                }
                return;

            case LineModel x:
                page.Contents.DrawLine(posx, posy, posx + x.Width, posy + x.Height, x.Color);
                return;

            case RectangleModel x:
                page.Contents.DrawRectangle(posx, posy, x.Width, x.Height, x.Color);
                return;

            case FillRectangleModel x:
                page.Contents.DrawFillRectangle(posx, posy, x.Width, x.Height, x.LineColor, x.FillColor);
                return;

            case ImageModel x:
                page.Contents.DrawImage(posx, posy, imageget(x), x.ZoomWidth, x.ZoomHeight);
                return;
        }
        throw new();
    }
}
