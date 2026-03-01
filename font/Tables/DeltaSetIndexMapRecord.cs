using Mina.Extension;
using System.IO;

namespace OpenType.Tables;

public class DeltaSetIndexMapRecord
{
    public required byte Format { get; init; }
    public required byte EntryFormat { get; init; }
    public required uint MapCount { get; init; }
    public required byte[] MapData { get; init; }

    public static DeltaSetIndexMapRecord ReadFrom(Stream stream)
    {
        var format = stream.ReadUByte();
        var entryFormat = stream.ReadUByte();
        var mapCount = format == 0 ? (uint)stream.ReadUShortByBigEndian() : stream.ReadUIntByBigEndian();

        var entrySize = ((entryFormat & (byte)EntryFormats.MAP_ENTRY_SIZE_MASK) >> 4) + 1;
        var mapData = stream.ReadExactly((int)(entrySize * mapCount));

        return new()
        {
            Format = format,
            EntryFormat = entryFormat,
            MapCount = mapCount,
            MapData = mapData,
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
        stream.WriteByte(EntryFormat);
        if (Format == 0)
        {
            stream.WriteUShortByBigEndian((ushort)MapCount);
        }
        else
        {
            stream.WriteUIntByBigEndian(MapCount);
        }
        stream.Write(MapData);
    }
}