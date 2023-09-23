using Extensions;
using PicoPDF.Pdf.Element;
using PicoPDF.Pdf.Font;
using PicoPDF.Pdf.XObject;
using System.Linq;

namespace PicoPDF.Pdf;

public class Page : PdfObject
{
    public required Document Document { get; init; }
    public PageSize Size { get; init; } = PageSize.A4;
    public Orientation Orientation { get; init; } = Orientation.Vertical;
    public Contents Contents { get; }

    public Page()
    {
        Contents = new Contents() { Page = this };
        RelatedObjects.Add(Contents);

        _ = Elements.TryAdd("Type", "/Page");
        _ = Elements.TryAdd("Contents", Contents);
    }

    public override void DoExport()
    {
        var (width, height) = PdfUtility.GetPageSize(Size, Orientation);
        _ = Elements.TryAdd("MediaBox", new long[] { 0, 0, width, height });

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

        var xobjs = Document.PdfObjects.OfType<ImageXObject>().ToArray();
        if (xobjs.Length > 0)
        {
            var xobjdic = new ElementDictionary();
            xobjs.Each(x => xobjdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x }));
            dic.Dictionary.Add("XObject", xobjdic);
        }
    }
}
