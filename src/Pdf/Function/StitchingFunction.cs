using Mina.Extension;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Extension;
using System.Linq;

namespace PicoPDF.Pdf.Function;

public class StitchingFunction : PdfObject, IFunction
{
    public FunctionTypes FunctionType { get; init; } = FunctionTypes.StitchingFunction;
    public required float[] Domain { get; init; }
    public required IFunction[] Functions { get; init; }
    public required float[] Bounds { get; init; }
    public required float[] Encode { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        Functions.OfType<PdfObject>().Each(RelatedObjects.Add);
        _ = Elements.TryAdd("FunctionType", (int)FunctionType);
        _ = Elements.TryAdd("Domain", $"[{Domain.Select(x => x.PointToString(option.PointFormat)).Join(" ")}]");
        _ = Elements.TryAdd("Functions", new ElementArray<ElementIndirectObject>(Functions.OfType<PdfObject>().Select(x => new ElementIndirectObject { References = x })));
        _ = Elements.TryAdd("Bounds", $"[{Bounds.Select(x => x.PointToString(option.PointFormat)).Join(" ")}]");
        _ = Elements.TryAdd("Encode", $"[{Encode.Select(x => x.PointToString(option.PointFormat)).Join(" ")}]");
    }
}
