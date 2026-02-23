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
        var clipBoxOffsets = new int[Clips.Length];
        _ = ClipBoxFormats
            .Select((x, i) => (ClipBoxFormat: x, Index: i))
            .Accumulator((acc, x) => (clipBoxOffsets[x.Index] = acc) + x.ClipBoxFormat.SizeOf(), Format.SizeOf() + NumberClips.SizeOf() + (/* sizeof(Clips) */8 * Clips.Length));

        stream.WriteByte(Format);
        stream.WriteUIntByBigEndian(NumberClips);
        for (int i = 0; i < Clips.Length; i++)
        {
            stream.WriteUShortByBigEndian(Clips[i].StartGlyphID);
            stream.WriteUShortByBigEndian(Clips[i].EndGlyphID);
            stream.Write3BytesByBigEndian(clipBoxOffsets[i]);
        }
        ClipBoxFormats.Each(x => x.WriteTo(stream));
    }
}
