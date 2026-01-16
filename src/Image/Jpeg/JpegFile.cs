using Mina.Extension;
using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PicoPDF.Image.Jpeg;

public class JpegFile : IImage
{
    public static readonly byte[] MagicNumber = [0xFF, 0xD8];
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required int Precision { get; init; }

    public static JpegFile FromStream(Stream stream)
    {
        var signature = stream.ReadExactly(2);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        while (true)
        {
            var header = stream.ReadExactly(4).AsSpan();
            if (header.Length < 4) break;

            var marker = BinaryPrimitives.ReadUInt16BigEndian(header[0..2]);
            var length = BinaryPrimitives.ReadUInt16BigEndian(header[2..4]) - 2;
            if (marker is
                    (ushort)SegmentTypes.SOF0 or
                    (ushort)SegmentTypes.SOF1 or
                    (ushort)SegmentTypes.SOF2 or
                    (ushort)SegmentTypes.SOF3 or
                    (ushort)SegmentTypes.SOF5 or
                    (ushort)SegmentTypes.SOF6 or
                    (ushort)SegmentTypes.SOF7 or
                    (ushort)SegmentTypes.SOF8 or
                    (ushort)SegmentTypes.SOF9 or
                    (ushort)SegmentTypes.SOF10 or
                    (ushort)SegmentTypes.SOF11 or
                    (ushort)SegmentTypes.SOF13 or
                    (ushort)SegmentTypes.SOF14 or
                    (ushort)SegmentTypes.SOF15
                )
            {
                var sof = stream.ReadExactly(5).AsSpan();
                var precision = sof[0];
                var height = BinaryPrimitives.ReadUInt16BigEndian(sof[1..3]);
                var width = BinaryPrimitives.ReadUInt16BigEndian(sof[3..5]);
                return new() { Width = width, Height = height, Precision = precision };
            }
            stream.Position += length;
        }
        throw new();
    }
}
