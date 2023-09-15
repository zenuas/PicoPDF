using Extensions;
using System.Buffers.Binary;
using System.IO;

namespace PicoPDF.TrueType;

public struct MaximumProfileTable
{
    public int Version;
    public ushort NumberOfGlyphs;
    public ushort MaxPoints;
    public ushort MaxContours;
    public ushort MaxCompositePoints;
    public ushort MaxCompositeContours;
    public ushort MaxZones;
    public ushort MaxTwilightPoints;
    public ushort MaxStorage;
    public ushort MaxFunctionDefs;
    public ushort MaxInstructionDefs;
    public ushort MaxStackElements;
    public ushort MaxSizeOfInstructions;
    public ushort MaxComponentElements;
    public ushort MaxComponentDepth;

    public static MaximumProfileTable ReadFrom(Stream stream)
    {
        var maxp = new MaximumProfileTable()
        {
            Version = BinaryPrimitives.ReadInt32BigEndian(stream.ReadBytes(4)),
            NumberOfGlyphs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2)),
        };

        if (maxp.Version == 0x00010000)
        {
            maxp.MaxPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxCompositePoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxCompositeContours = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxZones = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxTwilightPoints = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxStorage = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxFunctionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxInstructionDefs = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxStackElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxSizeOfInstructions = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxComponentElements = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
            maxp.MaxComponentDepth = BinaryPrimitives.ReadUInt16BigEndian(stream.ReadBytes(2));
        }

        return maxp;
    }
}
