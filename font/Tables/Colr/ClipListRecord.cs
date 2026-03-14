using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.Colr;

public class ClipListRecord : IExportable
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
        var clips = Lists.Repeat(() => (stream.ReadUShortByBigEndian(), stream.ReadUShortByBigEndian(), ClipBoxOffset: stream.ReadOffset24())).Take((int)numClips).ToArray();
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
            .Accumulator((acc, x) => (clipBoxOffsets[x.Index] = acc) + x.ClipBoxFormat.SizeOf(), Format.SizeOf() + NumberClips.SizeOf() + ((sizeof(ushort) + sizeof(ushort) + Const.SizeofOffset24) * Clips.Length))
            .ToArray();

        stream.WriteByte(Format);
        stream.WriteUIntByBigEndian((uint)Clips.Length);
        for (int i = 0; i < Clips.Length; i++)
        {
            stream.WriteUShortByBigEndian(Clips[i].StartGlyphID);
            stream.WriteUShortByBigEndian(Clips[i].EndGlyphID);
            stream.WriteOffset24(clipBoxOffsets[i]);
        }
        ClipBoxFormats.Each(x => x.WriteTo(stream));
    }

    public int SizeOf() => Format.SizeOf() + NumberClips.SizeOf() + ((sizeof(ushort) + sizeof(ushort) + Const.SizeofOffset24) * Clips.Length) + ClipBoxFormats.Select(x => x.SizeOf()).Sum();
}
