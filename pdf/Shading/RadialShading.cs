using Mina.Extension;
using Pdf.Drawing;
using Pdf.Elements;
using Pdf.Extension;
using Pdf.Function;
using Svg.Outline;
using System;
using System.Drawing;

namespace Pdf.Shading;

public class RadialShading : PdfObject, IShading
{
    public required string Name { get; init; }
    public ShadingTypes ShadingType { get; init; } = ShadingTypes.RadialShading;
    public string ColorSpace { get; init; } = "/DeviceRGB";
    public required (IPoint X0, IPoint Y0, IPoint R0, IPoint X1, IPoint Y1, IPoint R1) Coords { get; init; }
    public (float T0, float T1) Domain { get; init; } = (0.0f, 1.0f);
    public required IFunction Function { get; init; }
    public (bool B0, bool B1) Extend { get; init; } = (false, false);

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(Function.Cast<PdfObject>());
        _ = Elements.TryAdd("ShadingType", (int)ShadingType);
        _ = Elements.TryAdd("ColorSpace", ColorSpace);
        _ = Elements.TryAdd("Coords", $"[{new[] { Coords.X0, Coords.Y0, Coords.R0, Coords.X1, Coords.Y1, Coords.R1 }.ToPointString(option.PointFormat)}]");
        _ = Elements.TryAdd("Domain", $"[{new[] { Domain.T0, Domain.T1 }.ToPointString(option.PointFormat)}]");
        _ = Elements.TryAdd("Function", new ElementIndirectObject { References = Function.Cast<PdfObject>() });
        _ = Elements.TryAdd("Extend", $"[{Extend.B0.ToString().ToLower()} {Extend.B1.ToString().ToLower()}]");
    }

    public static RadialShading Create(string name, RadialGradientLayer radial, Func<Color, float[]> f, string color_space) => new()
    {
        Name = name,
        ColorSpace = color_space,
        Coords = (
                new PointValue(radial.Fxy.X),
                new PointValue(radial.Fxy.Y),
                new PointValue(radial.Fr),
                new PointValue(radial.Cxy.X),
                new PointValue(radial.Cxy.Y),
                new PointValue(radial.R)
            ),
        Function = StitchingFunction.FromStopColors(radial.StopColors, f),
        Extend = (true, true),
    };
}
