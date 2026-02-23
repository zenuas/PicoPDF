using Mina.Extension;
using PicoPDF.OpenType.Extension;
using System.IO;
using System.Linq;

namespace PicoPDF.OpenType.Tables;

public class ClipListRecord
{
    public required byte Format { get; init; }
    public required uint NumberClips { get; init; }
    public required (ushort StartGlyphID, ushort EndGlyphID, int ClipBoxOffset)[] Clips { get; init; }
    public required ClipBoxFormat[] ClipBoxFormats { get; init; }

    public static ClipListRecord ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var format = stream.ReadUByte();
        var numClips = stream.ReadUIntByBigEndian();
        var clips = Lists.Repeat(() => (stream.ReadUShortByBigEndian(), stream.ReadUShortByBigEndian(), ClipBoxOffset: stream.Read3BytesByBigEndian())).Take((int)numClips).ToArray();
        var clipBoxFormats = clips.Select(x => stream.SeekTo(position + x.ClipBoxOffset).To(ClipBoxFormat.ReadFrom)).ToArray();

        return new()
        {
            Format = format,
            NumberClips = numClips,
            Clips = clips,
            ClipBoxFormats = clipBoxFormats,
        };
    }

    public void WriteTo(Stream stream)
    {
    }
}
