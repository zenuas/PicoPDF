using Mina.Extension;
using Pdf.Drawing;
using Pdf.Elements;
using Pdf.Extension;
using Pdf.Operation;
using Pdf.Shading;
using Pdf.XObject.Group;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.XObject.Form;

public class FormXObject : PdfObject, IXObject
{
    public required string Name { get; init; }
    public required (IPoint Left, IPoint Bottom, IPoint Right, IPoint Top) BBox { get; init; }
    public IShading[] ResourcesShading { get; init; } = [];
    public GroupXObject? Group { get; init; } = null;
    public List<IOperation> Operations { get; init; } = [];

    public override void BeforeExport(PdfExportOption option)
    {
        ResourcesShading.OfType<PdfObject>().Each(RelatedObjects.Add);
        if (Group is { }) RelatedObjects.Add(Group);

        _ = Elements.TryAdd("Type", "/XObject");
        _ = Elements.TryAdd("Subtype", "/Form");
        _ = Elements.TryAdd("BBox", $"[{new[] { BBox.Left, BBox.Bottom, BBox.Right, BBox.Top }.ToPointString(option.PointFormat)}]");

        if (ResourcesShading.Length > 0)
        {
            var dic = new ElementDictionary();

            var shdic = new ElementDictionary();
            ResourcesShading.Each(x => shdic.Dictionary.TryAdd(x.Name, new ElementIndirectObject() { References = x.Cast<IPdfObject>() }));
            dic.Dictionary.Add("Shading", shdic);

            _ = Elements.TryAdd("Resources", dic);
        }
        if (Group is { }) _ = Elements.TryAdd("Group", new ElementIndirectObject() { References = Group });

        if (Operations.Count > 0)
        {
            var writer = GetWriteStream(option.ContentsStreamDeflate);
            Operations.Each(x => x.OperationWrite(0, 0, writer, option));
            writer.Flush();
        }
    }

    public static FormXObject Create(string name, float left, float bottom, float right, float top) => new()
    {
        Name = name,
        BBox = (new PointValue(left), new PointValue(bottom), new PointValue(right), new PointValue(top)),
    };

    public static FormXObject CreateTransparency(float left, float bottom, float right, float top, IShading shading) => new()
    {
        Name = "",
        BBox = (new PointValue(left), new PointValue(bottom), new PointValue(right), new PointValue(top)),
        ResourcesShading = [shading],
        Group = new GroupXObject
        {
            S = "/Transparency",
            I = true,
            CS = "/DeviceGray",
        },
    };
}
