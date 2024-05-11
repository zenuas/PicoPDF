using Mina.Extension;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public class MaximumProfileTable
{
    public readonly int Version;
    public readonly ushort NumberOfGlyphs;
    public readonly ushort MaxPoints;
    public readonly ushort MaxContours;
    public readonly ushort MaxCompositePoints;
    public readonly ushort MaxCompositeContours;
    public readonly ushort MaxZones;
    public readonly ushort MaxTwilightPoints;
    public readonly ushort MaxStorage;
    public readonly ushort MaxFunctionDefs;
    public readonly ushort MaxInstructionDefs;
    public readonly ushort MaxStackElements;
    public readonly ushort MaxSizeOfInstructions;
    public readonly ushort MaxComponentElements;
    public readonly ushort MaxComponentDepth;

    public MaximumProfileTable(Stream stream)
    {
        Version = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4));
        NumberOfGlyphs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));

        if (Version == 0x00010000)
        {
            MaxPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxCompositePoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxCompositeContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxZones = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxTwilightPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxStorage = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxFunctionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxInstructionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxStackElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxSizeOfInstructions = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxComponentElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            MaxComponentDepth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }
    }
}
