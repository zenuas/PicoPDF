using Mina.Extension;
using Pdf.Elements;
using Pdf.Extension;
using Pdf.SoftMasks;

namespace Pdf.ExtGState;

public class GraphicsStateParameter : PdfObject, IGraphicsStateParameter
{
    public required string Name { get; init; }
    public SoftMask? SMask { get; init; } = null;
    public float? CA { get; init; } = null;
    public float? Ca { get; init; } = null;
    public bool? AIS { get; init; } = null;

    public override void DoExport(PdfExportOption option)
    {
        if (SMask is { }) RelatedObjects.Add(SMask.Cast<PdfObject>());

        _ = Elements.TryAdd("Type", "/ExtGState");
        if (SMask is { } smask) _ = Elements.TryAdd("SMask", new ElementIndirectObject { References = smask });
        if (CA is { } ca1) _ = Elements.TryAdd("CA", ca1.ToPointString(option.PointFormat));
        if (Ca is { } ca2) _ = Elements.TryAdd("ca", ca2.ToPointString(option.PointFormat));
        if (AIS is { } ais) _ = Elements.TryAdd("AIS", ais.ToString().ToLower());
    }
}
