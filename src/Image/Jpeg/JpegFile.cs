using Extensions;
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
        var signature = stream.ReadBytes(2);
        Debug.Assert(MagicNumber.SequenceEqual(signature));

        while (true)
        {
            var header = stream.ReadBytes(4).AsSpan();
            if (header.Length < 4) break;

            var maeker = BinaryPrimitives.ReadUInt16BigEndian(header[0..2]);
            var length = BinaryPrimitives.ReadUInt16BigEndian(header[2..4]) - 2;
            if (maeker.In(
                    (ushort)SegmentTypes.SOF0,
                    (ushort)SegmentTypes.SOF1,
                    (ushort)SegmentTypes.SOF2,
                    (ushort)SegmentTypes.SOF3,
                    (ushort)SegmentTypes.SOF5,
                    (ushort)SegmentTypes.SOF6,
                    (ushort)SegmentTypes.SOF7,
                    (ushort)SegmentTypes.SOF8,
                    (ushort)SegmentTypes.SOF9,
                    (ushort)SegmentTypes.SOF10,
                    (ushort)SegmentTypes.SOF11,
                    (ushort)SegmentTypes.SOF13,
                    (ushort)SegmentTypes.SOF14,
                    (ushort)SegmentTypes.SOF15
                ))
            {
                var sof = stream.ReadBytes(5).AsSpan();
                var precision = sof[0];
                var height = BinaryPrimitives.ReadUInt16BigEndian(sof[1..3]);
                var width = BinaryPrimitives.ReadUInt16BigEndian(sof[3..5]);
                return new JpegFile() { Width = width, Height = height, Precision = precision };
            }
            stream.Position += length;
        }
        throw new();
    }
}
