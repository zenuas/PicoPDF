using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables.Subtable;

public class PairPosFormat2 : ISubtable, IPairPosition
{
    public required ushort Format { get; init; }
    public required Offset16 CoverageOffset { get; init; }
    public required ValueFormatFlags ValueFormat1 { get; init; }
    public required ValueFormatFlags ValueFormat2 { get; init; }
    public required Offset16 ClassDef1Offset { get; init; }
    public required Offset16 ClassDef2Offset { get; init; }
    public required ushort Class1Count { get; init; }
    public required ushort Class2Count { get; init; }
    public required (ValueRecord ValueRecord1, ValueRecord ValueRecord2)[][] ClassRecords { get; init; }
    public required ICoverageFormat Coverage { get; init; }
    public required IClassDefFormat ClassDef1 { get; init; }
    public required IClassDefFormat ClassDef2 { get; init; }

    public static PairPosFormat2 ReadFrom(Stream stream)
    {
        var position = stream.Position - sizeof(ushort);

        var coverage_offset = stream.ReadOffset16();
        var value_format1 = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var value_format2 = (ValueFormatFlags)stream.ReadUShortByBigEndian();
        var class_def1_offset = stream.ReadOffset16();
        var class_def2_offset = stream.ReadOffset16();
        var class1_count = stream.ReadUShortByBigEndian();
        var class2_count = stream.ReadUShortByBigEndian();

        var class1_records = new (ValueRecord, ValueRecord)[class1_count][];
        for (var c1 = 0; c1 < class1_count; c1++)
        {
            var class2_records = new (ValueRecord, ValueRecord)[class2_count];
            for (var c2 = 0; c2 < class2_count; c2++)
            {
                class2_records[c2] = (ValueRecord.ReadFrom(stream, value_format1), ValueRecord.ReadFrom(stream, value_format2));
            }
            class1_records[c1] = class2_records;
        }

        return new()
        {
            Format = 2,
            CoverageOffset = coverage_offset,
            ValueFormat1 = value_format1,
            ValueFormat2 = value_format2,
            ClassDef1Offset = class_def1_offset,
            ClassDef2Offset = class_def2_offset,
            Class1Count = class1_count,
            Class2Count = class2_count,
            ClassRecords = class1_records,
            Coverage = ICoverageFormat.ReadFrom(stream.SeekTo(position + coverage_offset.Value)),
            ClassDef1 = IClassDefFormat.ReadFrom(stream.SeekTo(position + class_def1_offset.Value)),
            ClassDef2 = IClassDefFormat.ReadFrom(stream.SeekTo(position + class_def2_offset.Value)),
        };
    }

    public (ValueRecord First, ValueRecord Second)? GetPosition(uint first_gid, uint second_gid)
    {
        if (Coverage.FindOrNull(first_gid) is null) return null;

        if (ClassDef1.GetClassValue(first_gid) is { } c1 &&
            ClassDef2.GetClassValue(second_gid) is { } c2) return ClassRecords[c1][c2];
        return null;
    }
}
