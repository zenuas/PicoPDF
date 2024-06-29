using Mina.Extension;
using System.IO;

namespace PicoPDF.OpenType;

public class MaximumProfileTable : IExportable
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
        var version = stream.ReadIntByBigEndian();
        var number_of_glyphs = stream.ReadUShortByBigEndian();

        return version != 0x00010000
            ? new() { Version = version, NumberOfGlyphs = number_of_glyphs }
            : new()
            {
                Version = version,
                NumberOfGlyphs = number_of_glyphs,
                MaxPoints = stream.ReadUShortByBigEndian(),
                MaxContours = stream.ReadUShortByBigEndian(),
                MaxCompositePoints = stream.ReadUShortByBigEndian(),
                MaxCompositeContours = stream.ReadUShortByBigEndian(),
                MaxZones = stream.ReadUShortByBigEndian(),
                MaxTwilightPoints = stream.ReadUShortByBigEndian(),
                MaxStorage = stream.ReadUShortByBigEndian(),
                MaxFunctionDefs = stream.ReadUShortByBigEndian(),
                MaxInstructionDefs = stream.ReadUShortByBigEndian(),
                MaxStackElements = stream.ReadUShortByBigEndian(),
                MaxSizeOfInstructions = stream.ReadUShortByBigEndian(),
                MaxComponentElements = stream.ReadUShortByBigEndian(),
                MaxComponentDepth = stream.ReadUShortByBigEndian(),
            };
    }

    public long WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteIntByBigEndian(Version);
        stream.WriteUShortByBigEndian(NumberOfGlyphs);

        if (Version == 0x00010000)
        {
            stream.WriteUShortByBigEndian(MaxPoints);
            stream.WriteUShortByBigEndian(MaxContours);
            stream.WriteUShortByBigEndian(MaxCompositePoints);
            stream.WriteUShortByBigEndian(MaxCompositeContours);
            stream.WriteUShortByBigEndian(MaxZones);
            stream.WriteUShortByBigEndian(MaxTwilightPoints);
            stream.WriteUShortByBigEndian(MaxStorage);
            stream.WriteUShortByBigEndian(MaxFunctionDefs);
            stream.WriteUShortByBigEndian(MaxInstructionDefs);
            stream.WriteUShortByBigEndian(MaxStackElements);
            stream.WriteUShortByBigEndian(MaxSizeOfInstructions);
            stream.WriteUShortByBigEndian(MaxComponentElements);
            stream.WriteUShortByBigEndian(MaxComponentDepth);
        }

        return stream.Position - position;
    }
}
