using Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct HorizontalMetrics
{
    public ushort AdvanceWidth;
    public short LeftSideBearing;

    public static HorizontalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceWidth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        LeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2)),
    };

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
