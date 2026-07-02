using Mina.Extension;
using OpenType.Extension;
using System.IO;
using System.Linq;

namespace OpenType.Tables.CMap;

public class CMapFormat14 : ICMapFormat, IExportable
{
    public required ushort Format { get; init; }
    public required uint Length { get; init; }
    public required uint NumberOfVariationSelectorRecords { get; init; }
    public required (int VariationSelector, Offset32 DefaultUVSOffset, Offset32 NonDefaultUVSOffset)[] VariationSelector { get; init; }

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

    public void WriteTo(Stream stream)
    {
        stream.WriteUShortByBigEndian(Format);
        stream.WriteUIntByBigEndian(Length);
        stream.WriteUIntByBigEndian(NumberOfVariationSelectorRecords);
        foreach (var x in VariationSelector)
        {
            stream.Write3BytesByBigEndian(x.VariationSelector);
            stream.WriteOffset32(x.DefaultUVSOffset);
            stream.WriteOffset32(x.NonDefaultUVSOffset);
        }
    }
}
