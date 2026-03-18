using Mina.Extension;
using OpenType.Extension;
using System;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat14 : ICMapFormat
{
    public required ushort Format { get; init; }
    public required uint Length { get; init; }
    public required uint NumberOfVariationSelectorRecords { get; init; }
    public required (int VariationSelector, uint DefaultUVSOffset, uint NonDefaultUVSOffset)[] VariationSelector { get; init; }

    public static CMapFormat14 ReadFrom(Stream stream)
    {
        var length = stream.ReadUIntByBigEndian();
        var numVarSelectorRecords = stream.ReadUIntByBigEndian();

        return new()
        {
            Format = 14,
            Length = length,
            NumberOfVariationSelectorRecords = numVarSelectorRecords,
            VariationSelector = [.. Lists.Repeat(() => (stream.Read3BytesByBigEndian(), stream.ReadOffset32(), stream.ReadOffset32())).Take((int)numVarSelectorRecords)],
        };
    }

    public static CMapFormat14 CreateFormat((int Char, uint GID)[] char_gids)
    {
        return new()
        {
            Format = (ushort)CMapFormats.Format14,
            Length = 0,
            NumberOfVariationSelectorRecords = 0,
            VariationSelector = [],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUIntByBigEndian(Length);
        stream.WriteUIntByBigEndian(NumberOfVariationSelectorRecords);
    }

    public Func<int, uint> CreateCharToGID()
    {
        return _ => 0;
    }
}
