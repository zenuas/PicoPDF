using System;
using System.Buffers.Binary;

namespace PicoPDF.OpenType.Extension;

public static class BinaryPrimitivesExtensions
{
    extension(BinaryPrimitives)
    {
        public static int ReadInt24LittleEndian(ReadOnlySpan<byte> source) => source[0] | (source[1] << 8) | (source[2] << 16);
        public static int ReadInt24BigEndian(ReadOnlySpan<byte> source) => (source[0] << 16) | (source[1] << 8) | source[2];
    }
}
