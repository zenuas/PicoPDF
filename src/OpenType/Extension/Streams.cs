using System;
using System.IO;

namespace PicoPDF.OpenType.Extension;

public static class Streams
{
    public static void Write3BytesByLittleEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24LittleEndian(buffer, n); self.Write(buffer); }
    public static void Write3BytesByBigEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24BigEndian(buffer, n); self.Write(buffer); }

    public static int Read3BytesByLittleEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return ReadInt24LittleEndian(buffer); }
    public static int Read3BytesByBigEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return ReadInt24BigEndian(buffer); }

    public static void WriteInt24LittleEndian(Span<byte> source, int value)
    {
        source[0] = (byte)(value & 0xFF);
        source[1] = (byte)((value >> 8) & 0xFF);
        source[2] = (byte)((value >> 16) & 0xFF);
    }

    public static void WriteInt24BigEndian(Span<byte> source, int value)
    {
        source[0] = (byte)((value >> 16) & 0xFF);
        source[1] = (byte)((value >> 8) & 0xFF);
        source[2] = (byte)(value & 0xFF);
    }

    public static int ReadInt24LittleEndian(ReadOnlySpan<byte> source) => source[0] | (source[1] << 8) | (source[2] << 16);
    public static int ReadInt24BigEndian(ReadOnlySpan<byte> source) => (source[0] << 16) | (source[1] << 8) | source[2];
}
