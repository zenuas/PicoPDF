using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class HorizontalMetrics
{
    public required ushort AdvanceWidth { get; init; }
    public required short LeftSideBearing { get; init; }

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = stream.ReadUShortByBigEndian(),
        LeftSideBearing = stream.ReadShortByBigEndian(),
    };

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
