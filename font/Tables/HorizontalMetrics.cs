using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class HorizontalMetrics : IExportable
{
    public required ushort AdvanceWidth { get; init; }
    public required short LeftSideBearing { get; init; }

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = stream.ReadUFWORD(),
        LeftSideBearing = stream.ReadFWORD(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUFWORD(AdvanceWidth);
        stream.WriteFWORD(LeftSideBearing);
    }

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
