using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Shading;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.XObject.Form;

public class FormXObject : PdfObject, IXObject
{
    public required (IPoint Left, IPoint Bottom, IPoint Right, IPoint Top) BBox { get; init; }
    public List<IShading> ResourcesShading { get; init; } = [];

    public override void DoExport(PdfExportOption option)
    {
        ResourcesShading.OfType<PdfObject>().Each(RelatedObjects.Add);
        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Form");
        _ = Elements.TryAdd("BBox", $"[{PdfUtility.PointToString(BBox.Left.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Bottom.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Right.ToPoint(), option.PointFormat)} {PdfUtility.PointToString(BBox.Top.ToPoint(), option.PointFormat)}]");

        if (ResourcesShading.Count > 0)
        {
            var dic = new ElementDictionary();

            var shdic = new ElementDictionary();
            ResourcesShading.Each(x => shdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("Shading", shdic);

            _ = Elements.TryAdd("Resources", dic);
        }
    }
}
