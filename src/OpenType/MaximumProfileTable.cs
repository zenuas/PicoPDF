using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.OpenType;

public class MaximumProfileTable
{
    public required int Version { get; init; }
    public required ushort NumberOfGlyphs { get; init; }
    public ushort MaxPoints { get; init; }
    public ushort MaxContours { get; init; }
    public ushort MaxCompositePoints { get; init; }
    public ushort MaxCompositeContours { get; init; }
    public ushort MaxZones { get; init; }
    public ushort MaxTwilightPoints { get; init; }
    public ushort MaxStorage { get; init; }
    public ushort MaxFunctionDefs { get; init; }
    public ushort MaxInstructionDefs { get; init; }
    public ushort MaxStackElements { get; init; }
    public ushort MaxSizeOfInstructions { get; init; }
    public ushort MaxComponentElements { get; init; }
    public ushort MaxComponentDepth { get; init; }

    public static MaximumProfileTable ReadFrom(Stream stream)
    {
        var version = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4));
        var number_of_glyphs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        return version != 0x00010000
            ? new() { Version = version, NumberOfGlyphs = number_of_glyphs }
            : new()
            {
                Version = version,
                NumberOfGlyphs = number_of_glyphs,
                MaxPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxCompositePoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxCompositeContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxZones = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxTwilightPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxStorage = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxFunctionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxInstructionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxStackElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxSizeOfInstructions = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxComponentElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
                MaxComponentDepth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
            };
    }
}
