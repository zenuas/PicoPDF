using Extensions;
using PicoPDF.Document;
using PicoPDF.Document.Font;
using PicoPDF.Model.Element;
using System;
using System.Collections.Generic;

namespace PicoPDF.Model;

public static class ModelMapping
{
    public static void Mapping(Document.Document doc, PageModel[] pages)
    {
        var ttf = doc.AddFont("TTF", doc.FontRegister.GetOrNull("Meiryo-Bold")!);
        pages.Each(x => Mapping(doc.NewPage(x.Size, x.Orientation).Contents, ttf, x.Models));
    }

    public static void Mapping(Contents contents, IFont font, List<SectionModel> models)
    {
        var top = 0;
        models.Each(x => Mapping(contents, font, x, (top += x.Height) - x.Height));
    }

    public static void Mapping(Contents contents, IFont font, SectionModel model, int top) => model.Elements.Each(x => Mapping(contents, font, x, top));

    public static void Mapping(Contents contents, IFont font, IModelElement model, int top)
    {
        var posx = model.X;
        var posy = model.Y + top;
        switch (model)
        {
            case TextModel x:
                contents.DrawString(x.Text, posx, posy, 10, font);
                return;
        }
        throw new Exception();
    }
}
