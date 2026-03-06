using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.IO;

namespace OpenType.Extension;

public static class Streams
{
    public static void Write3BytesByLittleEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24LittleEndian(buffer, n); self.Write(buffer); }
    public static void Write3BytesByBigEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteFixed(this Stream self, uint n) => self.WriteUIntByBigEndian(n);
    public static void WriteFWORD(this Stream self, short n) => self.WriteShortByBigEndian(n);
    public static void WriteUFWORD(this Stream self, ushort n) => self.WriteUShortByBigEndian(n);
    public static void WriteF2DOT14(this Stream self, ushort n) => self.WriteUShortByBigEndian(n);

    public static int Read3BytesByLittleEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return BinaryPrimitives.ReadInt24LittleEndian(buffer); }
    public static int Read3BytesByBigEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return BinaryPrimitives.ReadInt24BigEndian(buffer); }
    public static uint ReadFixed(this Stream self) => self.ReadUIntByBigEndian();
    public static short ReadFWORD(this Stream self) => self.ReadShortByBigEndian();
    public static ushort ReadUFWORD(this Stream self) => self.ReadUShortByBigEndian();
    public static ushort ReadF2DOT14(this Stream self) => self.ReadUShortByBigEndian();

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
}
