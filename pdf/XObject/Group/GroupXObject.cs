using System.Globalization;

namespace Pdf.XObject.Group;

public class GroupXObject : PdfObject, IXObject
{
    public string Name { get; init; } = "";
    public string S { get; init; } = "/Transparency";
    public bool I { get; init; } = false;
    public required string CS { get; init; }

    public override void BeforeExport(PdfExportOption option)
    {
        _ = Elements.TryAdd("Type", "/Group");
        _ = Elements.TryAdd("S", S);
        _ = Elements.TryAdd("I", I.ToString().ToLower(CultureInfo.InvariantCulture));
        _ = Elements.TryAdd("CS", CS);
    }
}
