using Mina.Extension;
using PicoPDF.Pdf.Drawing;
using PicoPDF.Pdf.Elements;
using PicoPDF.Pdf.Extension;
using PicoPDF.Pdf.Function;

namespace PicoPDF.Pdf.Shading;

public class AxialShading : PdfObject, IShading
{
    public required string Name { get; init; }
    public ShadingTypes ShadingType { get; init; } = ShadingTypes.AxialShading;
    public string ColorSpace { get; init; } = "/DeviceRGB";
    public required (IPoint X0, IPoint Y0, IPoint X1, IPoint Y1) Coords { get; init; }
    public (float T0, float T1) Domain { get; init; } = (0.0f, 1.0f);
    public required IFunction Function { get; init; }
    public (bool B0, bool B1) Extend { get; init; } = (false, false);

    public override void DoExport(PdfExportOption option)
    {
        RelatedObjects.Add(Function.Cast<PdfObject>());
        _ = Elements.TryAdd("ShadingType", (int)ShadingType);
        _ = Elements.TryAdd("ColorSpace", ColorSpace);
        _ = Elements.TryAdd("Coords", $"[{(new[] { Coords.X0, Coords.Y0, Coords.X1, Coords.Y1 }).ToPointString(option.PointFormat)}]");
        _ = Elements.TryAdd("Domain", $"[{Domain.T0.ToPointString(option.PointFormat)} {Domain.T1.ToPointString(option.PointFormat)}]");
        _ = Elements.TryAdd("Function", new ElementIndirectObject { References = Function.Cast<PdfObject>() });
        _ = Elements.TryAdd("Extend", $"[{Extend.B0.ToString().ToLower()} {Extend.B1.ToString().ToLower()}]");
    }
}
