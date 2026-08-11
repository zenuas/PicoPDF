using Pdf.Elements;
using Pdf.Extension;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Font;

public class CIDFontDictionary : PdfObject
{
    public required string Subtype { get; init; }
    public required string BaseFont { get; init; }
    public required ElementDictionary CIDSystemInfo { get; init; }
    public required FontDescriptor? FontDescriptor { get; init; }
    public double? DW { get; init; }
    public Dictionary<uint, (double Width, string Char)>? W { get; init; }
    public (double Top, double Height)? DW2 { get; init; }
    public Dictionary<uint, (double Height, double Right, double Top, string Char)>? W2 { get; set; }

    public override void BeforeExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/Font");
        _ = Elements.TryAdd("Subtype", $"/{Subtype}");
        _ = Elements.TryAdd("BaseFont", $"/{BaseFont}");
        _ = Elements.TryAdd("CIDSystemInfo", CIDSystemInfo);
        if (FontDescriptor is { } descriptor) _ = Elements.TryAdd("FontDescriptor", descriptor);
        if (DW is { } dw) _ = Elements.TryAdd("DW", dw.ToPointString(option.PointFormat));
        if (W is { } w)
        {
            _ = Elements.TryAdd("W", new ElementArray<ElementLiteral>([.. w.Keys
                .Where(gid => gid != 0 && (DW is not { } dw || w[gid].Width != dw))
                .Order()
                .Select(gid => new ElementLiteral { Value = $"{gid}[{w[gid].Width.ToPointString(option.PointFormat)}]{(option.Debug ? $" %{w[gid].Char}" : "")}\n" })]));
        }
        if (DW2 is { } dw2) _ = Elements.TryAdd("DW2", new string[] { dw2.Top.ToPointString(option.PointFormat), dw2.Height.ToPointString(option.PointFormat) });
        if (W2 is { } w2)
        {
            _ = Elements.TryAdd("W2", new ElementArray<ElementLiteral>([.. w2.Keys
                .Where(gid => gid != 0)
                .Order()
                .Select(gid => new ElementLiteral { Value = $"{gid}[{w2[gid].Height.ToPointString(option.PointFormat)} {w2[gid].Right.ToPointString(option.PointFormat)} {w2[gid].Top.ToPointString(option.PointFormat)}]{(option.Debug ? $" %{w2[gid].Char}" : "")}\n" })]));
        }
    }
}
