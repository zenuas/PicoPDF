using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class VerticalMetrics : IExportable
{
    public required UFWORD AdvanceHeight { get; init; }
    public required FWORD TopSideBearing { get; init; }

    public static VerticalMetrics ReadFrom(Stream stream) => new()
    {
        AdvanceHeight = stream.ReadUFWORD(),
        TopSideBearing = stream.ReadFWORD(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteUFWORD(AdvanceHeight);
        stream.WriteFWORD(TopSideBearing);
    }

    public override string ToString() => $"AdvanceHeight={AdvanceHeight}, TopSideBearing={TopSideBearing}";
}
