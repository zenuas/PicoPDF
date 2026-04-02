using Mina.Extension;
using OpenType.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace OpenType.Tables.TrueType;

public class LazyGlyph : IReadOnlyList<IGlyph>, IDisposable
{
    public IGlyph this[int index] => GlyphLoad((uint)index);
    public required Stream Stream { get; init; }
    public required int Count { get; init; }
    public required long IndexToLocationTableOffset { get; init; }
    public required long GlyphTableOffset { get; init; }
    public required short IndexToLocFormat { get; init; }
    public Dictionary<uint, IGlyph> GlyphCache { get; init; } = [];
    public bool Disposed { get; private set; } = false;
    private readonly Lock LockObject = new();

    public IGlyph GlyphLoad(uint gid)
    {
        if (gid >= Count) return GlyphLoad(0);
        lock (LockObject)
        {
            if (GlyphCache.TryGetValue(gid, out var cache)) return cache;

            Stream.Seek(IndexToLocationTableOffset + (IndexToLocFormat == 0 ? gid * 2 : gid * 4), SeekOrigin.Begin);
            var glyph_offset = IndexToLocFormat == 0 ? Stream.ReadOffset16() : Stream.ReadOffset32();
            var next_offset = IndexToLocFormat == 0 ? Stream.ReadOffset16() : Stream.ReadOffset32();

            if (glyph_offset == next_offset) return new NotdefGlyph();

            var number_of_contours = Stream.SeekTo(GlyphTableOffset + glyph_offset).ReadShortByBigEndian();
            var glyph = number_of_contours >= 0
                ? SimpleGlyph.ReadFrom(Stream, number_of_contours).Cast<IGlyph>()
                : CompositeGlyph.ReadFrom(Stream, number_of_contours);

            GlyphCache.Add(gid, glyph);
            return glyph;
        }
    }

    public IEnumerator<IGlyph> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        if (!Disposed)
        {
            lock (LockObject)
            {
                Stream.Dispose();
            }
            Disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~LazyGlyph() => Dispose();
}
