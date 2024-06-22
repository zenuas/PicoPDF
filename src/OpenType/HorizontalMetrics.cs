using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class HorizontalMetrics
{
    public required ushort AdvanceWidth { get; init; }
    public required short LeftSideBearing { get; init; }

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        LeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
    };

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
