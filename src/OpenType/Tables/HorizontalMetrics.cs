using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType.Tables;

public class HorizontalMetrics : IExportable
{
    public required ushort AdvanceWidth { get; init; }
    public required short LeftSideBearing { get; init; }

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = stream.ReadUShortByBigEndian(),
        LeftSideBearing = stream.ReadShortByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(AdvanceWidth);
        stream.WriteShortByBigEndian(LeftSideBearing);
    }

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
