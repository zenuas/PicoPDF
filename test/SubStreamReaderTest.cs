using OpenType.Extension;
using System;
using System.IO;
using Xunit;

namespace PicoPDF.Test;

public class SubStreamReaderTest
{
    public static Stream Init(int length)
    {
        var mem = new MemoryStream();
        for (var i = 0; i < length; i++) mem.WriteByte((byte)i);
        mem.Position = 0;
        return mem;
    }

    public static Stream InitLimit(int length, int start, int limit_length)
    {
        var mem = Init(length);
        var sub = new SubStreamReader { BaseStream = mem, StartOffset = start, LimitLength = limit_length };
        sub.Position = start;
        return sub;
    }

    [Fact]
    public void NormalRead()
    {
        var mem = Init(5);

        var bytes = new byte[100];
        var readed = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed, 5);
        Assert.Equal(bytes[0..5], [0x00, 0x01, 0x02, 0x03, 0x04]);
        Assert.Equal(mem.Position, 5);

        var readed2 = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed2, 0);

        mem.Position = 6;
        var readed3 = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed3, 0);

        mem.Position = 0;
        var readed4 = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed4, 5);
        Assert.Equal(bytes[0..5], [0x00, 0x01, 0x02, 0x03, 0x04]);

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Position = -1);
        mem.Position = 0;
        mem.Position = 1;
        mem.Position = 2;
        mem.Position = 3;
        mem.Position = 4;
        mem.Position = 5;
        mem.Position = 6;

        _ = Assert.Throws<IOException>(() => mem.Seek(-1, SeekOrigin.Begin));
        Assert.Equal(mem.Seek(0, SeekOrigin.Begin), 0);
        Assert.Equal(mem.Seek(1, SeekOrigin.Begin), 1);
        Assert.Equal(mem.Seek(3, SeekOrigin.Begin), 3);
        Assert.Equal(mem.Seek(4, SeekOrigin.Begin), 4);
        Assert.Equal(mem.Seek(5, SeekOrigin.Begin), 5);
        Assert.Equal(mem.Seek(6, SeekOrigin.Begin), 6);

        mem.Position = 0;
        _ = Assert.Throws<IOException>(() => mem.Seek(-1, SeekOrigin.Current));
        Assert.Equal(mem.Seek(0, SeekOrigin.Current), 0);
        Assert.Equal(mem.Seek(1, SeekOrigin.Current), 1);
        Assert.Equal(mem.Seek(4, SeekOrigin.Current), 5);
        Assert.Equal(mem.Seek(5, SeekOrigin.Current), 10);
        Assert.Equal(mem.Seek(6, SeekOrigin.Current), 16);

        Assert.Equal(mem.Seek(0, SeekOrigin.End), 5);
        Assert.Equal(mem.Seek(1, SeekOrigin.End), 6);
        Assert.Equal(mem.Seek(-1, SeekOrigin.End), 4);
        Assert.Equal(mem.Seek(-2, SeekOrigin.End), 3);
        Assert.Equal(mem.Seek(-3, SeekOrigin.End), 2);
        Assert.Equal(mem.Seek(-4, SeekOrigin.End), 1);
        Assert.Equal(mem.Seek(-5, SeekOrigin.End), 0);
        _ = Assert.Throws<IOException>(() => mem.Seek(-6, SeekOrigin.End));

        _ = Assert.Throws<ArgumentException>(() => mem.Seek(0, (SeekOrigin)3));

        for (var i = 0; i <= 7; i++)
        {
            mem.Position = 0;
            var readed5 = mem.Read(bytes, 0, i);
            Assert.Equal(readed5, i <= 5 ? i : 5);
        }
    }

    [Fact]
    public void LimitRead()
    {
        var mem = InitLimit(5, 1, 3);

        var bytes = new byte[100];
        var readed = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed, 3);
        Assert.Equal(bytes[0..3], [0x01, 0x02, 0x03]);
        Assert.Equal(mem.Position, 4);

        var readed2 = mem.Read(bytes, 0, bytes.Length);
        Assert.Equal(readed2, 0);

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Position = -1);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Position = 0);
        mem.Position = 1;
        mem.Position = 2;
        mem.Position = 3;
        mem.Position = 4;
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Position = 5);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Position = 6);

        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(-1, SeekOrigin.Begin));
        Assert.Equal(mem.Seek(0, SeekOrigin.Begin), 1);
        Assert.Equal(mem.Seek(1, SeekOrigin.Begin), 2);
        Assert.Equal(mem.Seek(3, SeekOrigin.Begin), 4);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(5, SeekOrigin.Begin));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(6, SeekOrigin.Begin));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(7, SeekOrigin.Begin));

        mem.Position = 1;
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(-1, SeekOrigin.Current));
        Assert.Equal(mem.Seek(0, SeekOrigin.Current), 1);
        Assert.Equal(mem.Seek(1, SeekOrigin.Current), 2);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(4, SeekOrigin.Current));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(5, SeekOrigin.Current));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(6, SeekOrigin.Current));

        Assert.Equal(mem.Seek(0, SeekOrigin.End), 4);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(1, SeekOrigin.End));
        Assert.Equal(mem.Seek(-1, SeekOrigin.End), 3);
        Assert.Equal(mem.Seek(-2, SeekOrigin.End), 2);
        Assert.Equal(mem.Seek(-3, SeekOrigin.End), 1);
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(-4, SeekOrigin.End));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(-5, SeekOrigin.End));
        _ = Assert.Throws<ArgumentOutOfRangeException>(() => mem.Seek(-6, SeekOrigin.End));

        _ = Assert.Throws<ArgumentException>(() => mem.Seek(0, (SeekOrigin)3));

        for (var i = 0; i <= 7; i++)
        {
            mem.Position = 1;
            var readed5 = mem.Read(bytes, 0, i);
            Assert.Equal(readed5, i <= 3 ? i : 3);
        }
    }
}
