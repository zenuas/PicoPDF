using System;
using System.IO;

namespace OpenType.Extension;

public class SubStreamReader : Stream
{
    public required Stream BaseStream { protected get; init; }
    public required long StartOffset { get; init; }
    public required long LimitLength { get; init; }

    public override bool CanRead => BaseStream.CanRead;
    public override bool CanSeek => BaseStream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => BaseStream.Length;
    public override long Position
    {
        get => BaseStream.Position;
        set
        {
            if (value < StartOffset || value > StartOffset + LimitLength) throw new ArgumentOutOfRangeException(nameof(value));
            BaseStream.Position = value;
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var pos = Position;
        if (pos < StartOffset) return 0;

        var max = StartOffset + LimitLength;
        if (pos >= max) return 0;

        return BaseStream.Read(buffer, offset, pos + count >= max ? (int)(max - pos) : count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return Position = offset + origin switch
        {
            SeekOrigin.Begin => StartOffset,
            SeekOrigin.Current => Position,
            SeekOrigin.End => StartOffset + LimitLength,
            _ => throw new ArgumentException("Invalid seek origin.", nameof(origin)),
        };
    }

    public override void Flush() => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
