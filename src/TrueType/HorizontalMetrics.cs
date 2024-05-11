using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public class HorizontalMetrics(Stream stream)
{
    public readonly ushort AdvanceWidth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
    public readonly short LeftSideBearing = BinaryPrimitives.ReadInt16BigEndian(stream.ReadBytes(2));

    public override string ToString() => $"AdvanceWidth={AdvanceWidth}, LeftSideBearing={LeftSideBearing}";
}
