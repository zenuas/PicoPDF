using Extensions;
using PicoPDF.Binder.Element;
using PicoPDF.Model.Element;
using PicoPDF.Pdf;
using PicoPDF.Pdf.Font;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document doc, PageModel[] pages)
    {
        var fontcache = doc.ResourcesFont.Values.OfType<TrueTypeFont>().ToDictionary(x => x.Name, x => x);
        TrueTypeFont fontget(string name)
        {
            if (fontcache.ContainsKey(name)) return fontcache[name];
            var x = doc.AddFont($"F{fontcache.Count}", doc.FontRegister.GetOrNull(name)!);
            fontcache.Add(name, x);
            return x;
        }
        pages.Each(x => Mapping(doc.NewPage(x.Size, x.Orientation), fontget, x.Models));
    }

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, List<SectionModel> models)
    {
        models.Each(x => Mapping(page, fontget, x, x.Top));
    }

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, SectionModel model, int top) => model.Elements.Each(x => Mapping(page, fontget, x, top));

    public static void Mapping(Page page, Func<string, TrueTypeFont> fontget, IModelElement model, int top)
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
        }
        throw new();
    }
}
