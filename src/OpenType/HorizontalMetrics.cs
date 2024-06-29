using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class HorizontalMetrics : IExportable
{
    public required ushort AdvanceWidth { get; init; }
    public required short LeftSideBearing { get; init; }

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = stream.ReadUShortByBigEndian(),
        LeftSideBearing = stream.ReadShortByBigEndian(),
    };

    public long WriteTo(Stream stream)
    {
        var position = stream.Position;
        stream.WriteUShortByBigEndian(AdvanceWidth);
        stream.WriteShortByBigEndian(LeftSideBearing);
        return stream.Position - position;
    }

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
