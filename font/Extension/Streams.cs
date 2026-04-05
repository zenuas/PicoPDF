using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace OpenType.Extension;

public static class Streams
{
    public static void Write3BytesByLittleEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24LittleEndian(buffer, n); self.Write(buffer); }
    public static void Write3BytesByBigEndian(this Stream self, int n) { Span<byte> buffer = stackalloc byte[3]; WriteInt24BigEndian(buffer, n); self.Write(buffer); }
    public static void WriteUInt24(this Stream self, int n) => self.Write3BytesByBigEndian(n);
    public static void WriteOffset8(this Stream self, byte n) => self.WriteByte(n);
    public static void WriteOffset16(this Stream self, ushort n) => self.WriteUShortByBigEndian(n);
    public static void WriteOffset24(this Stream self, int n) => self.Write3BytesByBigEndian(n);
    public static void WriteOffset32(this Stream self, uint n) => self.WriteUIntByBigEndian(n);
    public static void WriteFixed(this Stream self, uint n) => self.WriteUIntByBigEndian(n);
    public static void WriteFWORD(this Stream self, short n) => self.WriteShortByBigEndian(n);
    public static void WriteUFWORD(this Stream self, ushort n) => self.WriteUShortByBigEndian(n);
    public static void WriteF2DOT14(this Stream self, F2DOT14 n) => self.WriteUShortByBigEndian(n.Value);
    public static void WriteLONGDATETIME(this Stream self, LONGDATETIME n) => self.WriteLongByBigEndian(n.Value);
    public static void WriteTag(this Stream self, string s) => self.Write(s);

    public static int Read3BytesByLittleEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return BinaryPrimitives.ReadInt24LittleEndian(buffer); }
    public static int Read3BytesByBigEndian(this Stream self) { Span<byte> buffer = stackalloc byte[3]; self.ReadExactly(buffer); return BinaryPrimitives.ReadInt24BigEndian(buffer); }
    public static int ReadUInt24(this Stream self) => self.Read3BytesByBigEndian();
    public static byte ReadOffset8(this Stream self) => self.ReadUByte();
    public static ushort ReadOffset16(this Stream self) => self.ReadUShortByBigEndian();
    public static int ReadOffset24(this Stream self) => self.Read3BytesByBigEndian();
    public static uint ReadOffset32(this Stream self) => self.ReadUIntByBigEndian();
    public static uint ReadFixed(this Stream self) => self.ReadUIntByBigEndian();
    public static short ReadFWORD(this Stream self) => self.ReadShortByBigEndian();
    public static ushort ReadUFWORD(this Stream self) => self.ReadUShortByBigEndian();
    public static F2DOT14 ReadF2DOT14(this Stream self) => self.ReadUShortByBigEndian();
    public static LONGDATETIME ReadLONGDATETIME(this Stream self) => self.ReadLongByBigEndian();
    public static string ReadTag(this Stream self) => Encoding.ASCII.GetString(self.ReadExactly(4));

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
