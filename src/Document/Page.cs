using Extensions;
using PicoPDF.Document.Element;

namespace PicoPDF.Document;

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
        dic.Dictionary.Add("ProcSet", new string[] { "/PDF", "/Text" });

        if (Document.ResourcesFont.Count > 0)
        {
            var fontdic = new ElementDictionary();
            Document.ResourcesFont.Each(x => fontdic.Dictionary.TryAdd(x.Key, new ElementIndirectObject() { References = x.Value }));
            dic.Dictionary.Add("Font", fontdic);
        }
    }
}
