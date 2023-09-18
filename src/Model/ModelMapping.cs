using Extensions;
using PicoPDF.Document;
using PicoPDF.Document.Font;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document.Document doc, PageModel[] pages)
    {
        var fontcache = new Dictionary<string, TrueTypeFont>();
        TrueTypeFont fontget(string name)
        {
            if (fontcache.ContainsKey(name)) return fontcache[name];
            var x = doc.AddFont($"F{fontcache.Count}", doc.FontRegister.GetOrNull(name)!);
            fontcache.Add(name, x);
            return x;
        }
        fontcache.Add("", pages
            .Select(x => x.DefaultFont)
            .Distinct()
            .Select(fontget)
            .ToArray()
            .First());
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
                page.Contents.DrawString(x.Text, posx, posy, x.Size, fontget(x.Font));
                return;
        }
        throw new();
    }
}
