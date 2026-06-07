using Mina.Extension;
using Pdf.Elements;
using Pdf.ExtGState;
using Pdf.Font;
using Pdf.Operation;
using Pdf.Shading;
using Pdf.XObject.Image;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Documents;

public class Page : PdfObject
{
    public required Document Document { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public Contents Contents { get; }

    public static readonly string[] ProcSet = ["/PDF", "/Text", "/ImageB", "/ImageC", "/ImageI"];

    public Page()
    {
        Contents = new() { Page = this };
        RelatedObjects.Add(Contents);

        _ = Elements.TryAdd("Type", "/Page");
        _ = Elements.TryAdd("Contents", Contents);
    }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("MediaBox", new long[] { 0, 0, Width, Height });

        var dic = new ElementDictionary();
        _ = Elements.TryAdd("Resources", dic);
        dic.Dictionary.Add("ProcSet", ProcSet);

        var page_fonts = new HashSet<IFont>();
        var page_images = new HashSet<IImageXObject>();
        var page_shadings = new HashSet<IShading>();
        var page_extgstates = new HashSet<IGraphicsStateParameter>();
        foreach (var ope in Contents.EnumOperations(Contents.Operations))
        {
            switch (ope)
            {
                case DrawString x:
                    _ = page_fonts.Add(x.Font);
                    break;

                case DrawImage x:
                    _ = page_images.Add(x.Image);
                    break;

                case DrawPathShading x:
                    _ = page_shadings.Add(x.Shading);
                    break;

                case DrawPathExtGState x:
                    _ = page_extgstates.Add(x.ExtGState);
                    break;
            }
        }

        var fonts = Document.PdfObjects.OfType<IFont>().Where(page_fonts.Contains);
        if (fonts.Any())
        {
            var fontdic = new ElementDictionary();
            fonts.Each(x => fontdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x }));
            dic.Dictionary.Add("Font", fontdic);
        }

        var xobjs = Document.PdfObjects.OfType<IImageXObject>().Where(page_images.Contains);
        if (xobjs.Any())
        {
            var xobjdic = new ElementDictionary();
            xobjs.Each(x => xobjdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("XObject", xobjdic);
        }

        var shs = Document.PdfObjects.OfType<IShading>().Where(page_shadings.Contains);
        if (shs.Any())
        {
            var shdic = new ElementDictionary();
            shs.Each(x => shdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("Shading", shdic);
        }

        var gss = Document.PdfObjects.OfType<IGraphicsStateParameter>().Where(page_extgstates.Contains);
        if (gss.Any())
        {
            var gsdic = new ElementDictionary();
            gss.Each(x => gsdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("ExtGState", gsdic);
        }
    }
}
