using Mina.Extension;
using PicoPDF.Pdf.Element;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System.Linq;

namespace PicoPDF.Pdf;

public class Page : PdfObject
{
    public required Document Document { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public Contents Contents { get; }

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
        dic.Dictionary.Add("ProcSet", new string[] { "/PDF", "/Text", "/ImageB", "/ImageC", "/ImageI" });

        var fonts = Document.PdfObjects.OfType<IFont>().ToArray();
        if (fonts.Length > 0)
        {
            var fontdic = new ElementDictionary();
            fonts.Each(x => fontdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x }));
            dic.Dictionary.Add("Font", fontdic);
        }

        var xobjs = Document.PdfObjects.OfType<IImageXObject>().ToArray();
        if (xobjs.Length > 0)
        {
            var xobjdic = new ElementDictionary();
            xobjs.Each(x => xobjdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("XObject", xobjdic);
        }
    }
}
