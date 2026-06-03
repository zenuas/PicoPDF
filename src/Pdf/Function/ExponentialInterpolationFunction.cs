using Mina.Extension;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Extension;
using System.Linq;

namespace PicoPDF.Pdf.Function;

public class ExponentialInterpolationFunction : PdfObject, IFunction
{
    public FunctionTypes FunctionType { get; init; } = FunctionTypes.ExponentialInterpolationFunction;
    public required float[] Domain { get; init; }
    public float[] C0 { get; init; } = [0.0f];
    public float[] C1 { get; init; } = [1.0f];
    public required int N { get; init; }

    public override void DoExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("FunctionType", (int)FunctionType);
        _ = Elements.TryAdd("Domain", new ElementLiteral() { Value = $"[{Domain.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]" });
        _ = Elements.TryAdd("C0", new ElementLiteral() { Value = $"[{C0.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]" });
        _ = Elements.TryAdd("C1", new ElementLiteral() { Value = $"[{C1.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]" });
        _ = Elements.TryAdd("N", N);
    }
}
