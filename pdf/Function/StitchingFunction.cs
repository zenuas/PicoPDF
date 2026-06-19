using Mina.Extension;
using Pdf.Elements;
using Pdf.Extension;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Pdf.Function;

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
        _ = Elements.TryAdd("Domain", $"[{Domain.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]");
        _ = Elements.TryAdd("Functions", new ElementArray<ElementIndirectObject>(Functions.OfType<PdfObject>().Select(x => new ElementIndirectObject { References = x })));
        _ = Elements.TryAdd("Bounds", $"[{Bounds.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]");
        _ = Elements.TryAdd("Encode", $"[{Encode.Select(x => x.ToPointString(option.PointFormat)).Join(" ")}]");
    }

    public static StitchingFunction FromStopColors((float Offset, Color StopColor)[] stops, Func<Color, float[]> f)
    {
        Debug.Assert(stops.Length > 0);
        var exponentials = new ExponentialInterpolationFunction[stops.Length];
        var bounds = new float[stops.Length - 1];
        var encode = new float[stops.Length * 2];

        exponentials[0] = new ExponentialInterpolationFunction
        {
            Domain = [0.0f, 1.0f],
            C0 = f(stops[0].StopColor),
            C1 = f(stops[0].StopColor),
            N = 1,
        };
        encode[0] = 0.0f;
        encode[1] = 1.0f;
        for (var i = 0; i < stops.Length - 1; i++)
        {
            var (offset0, color0) = stops[i];
            var (_, color1) = stops[i + 1];
            exponentials[i + 1] = new ExponentialInterpolationFunction
            {
                Domain = [0.0f, 1.0f],
                C0 = f(color0),
                C1 = f(color1),
                N = 1,
            };
            bounds[i] = offset0 / 100.0f;
            encode[(i * 2) + 2] = 0.0f;
            encode[(i * 2) + 3] = 1.0f;
        }
        return new StitchingFunction
        {
            Domain = [0.0f, 1.0f],
            Functions = exponentials,
            Bounds = bounds,
            Encode = encode,
        };
    }
}
