using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Extension;
using PicoPDF.Pdf.Shading;
using PicoPDF.Pdf.XObject.Group;
using System.Linq;

namespace PicoPDF.Pdf.XObject.Form;

public class FormXObject : PdfObject, IXObject
{
    public required (IPoint Left, IPoint Bottom, IPoint Right, IPoint Top) BBox { get; init; }
    public IShading[] ResourcesShading { get; init; } = [];
    public GroupXObject? Group { get; init; } = null;

    public override void DoExport(PdfExportOption option)
    {
        ResourcesShading.OfType<PdfObject>().Each(RelatedObjects.Add);
        if (Group is { }) RelatedObjects.Add(Group);

        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Form");
        _ = Elements.TryAdd("BBox", $"[{Format.PointToString(BBox.Left.ToPoint(), option.PointFormat)} {Format.PointToString(BBox.Bottom.ToPoint(), option.PointFormat)} {Format.PointToString(BBox.Right.ToPoint(), option.PointFormat)} {Format.PointToString(BBox.Top.ToPoint(), option.PointFormat)}]");

        if (ResourcesShading.Length > 0)
        {
            var dic = new ElementDictionary();

            var shdic = new ElementDictionary();
            ResourcesShading.Each(x => shdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("Shading", shdic);

            _ = Elements.TryAdd("Resources", dic);
        }
        if (Group is { }) _ = Elements.TryAdd("Group", new ElementIndirectObject() { References = Group });
    }
}
