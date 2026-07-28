using Mina.Extension;
using Pdf.Elements;
using Pdf.Operation;
using System.Linq;

namespace Pdf.Documents;

public class Page : PdfObject
{
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

    public override void BeforeExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("MediaBox", new long[] { 0, 0, Width, Height });

        var dic = new ElementDictionary();
        _ = Elements.TryAdd("Resources", dic);
        dic.Dictionary.Add("ProcSet", ProcSet);

        var fontdic = new ElementDictionary();
        var xobjdic = new ElementDictionary();
        var shdic = new ElementDictionary();
        var gsdic = new ElementDictionary();
        foreach (var ope in Contents.EnumOperations(Contents.Operations))
        {
            switch (ope)
            {
                case DrawString x:
                    _ = fontdic.Dictionary.TryAdd(x.Font.Name, new ElementIndirectObject() { References = x.Font.Cast<IPdfObject>() });
                    break;

                case DrawPathXObject x:
                    _ = xobjdic.Dictionary.TryAdd(x.XObject.Name, new ElementIndirectObject() { References = x.XObject.Cast<IPdfObject>() });
                    break;

                case DrawImage x:
                    _ = xobjdic.Dictionary.TryAdd(x.Image.Name, new ElementIndirectObject() { References = x.Image.Cast<IPdfObject>() });
                    break;

                case DrawPathShading x:
                    _ = shdic.Dictionary.TryAdd(x.Shading.Name, new ElementIndirectObject() { References = x.Shading.Cast<IPdfObject>() });
                    break;

                case DrawPathExtGState x:
                    _ = gsdic.Dictionary.TryAdd(x.ExtGState.Name, new ElementIndirectObject() { References = x.ExtGState.Cast<IPdfObject>() });
                    break;
            }
        }

        if (fontdic.Dictionary.Count > 0) dic.Dictionary.Add("Font", fontdic);
        if (xobjdic.Dictionary.Count > 0) dic.Dictionary.Add("XObject", xobjdic);
        if (shdic.Dictionary.Count > 0) dic.Dictionary.Add("Shading", shdic);
        if (gsdic.Dictionary.Count > 0) dic.Dictionary.Add("ExtGState", gsdic);
    }
}
