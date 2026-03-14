using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables.Colr;
using System.Collections.Generic;
using System.IO;

namespace OpenType.Tables;

public class VarAffine2x3 : IExportable, IAffine2x3
{
    public required uint XX { get; init; }
    public required uint YX { get; init; }
    public required uint XY { get; init; }
    public required uint YY { get; init; }
    public required uint DX { get; init; }
    public required uint DY { get; init; }
    public required uint VarIndexBase { get; init; }

    public static VarAffine2x3 ReadFrom(Stream stream, long position, Dictionary<long, IAffine2x3> colorLineCache) => (VarAffine2x3)(colorLineCache.TryGetValue(position, out var line) ? line : colorLineCache[position] = ReadFrom(stream.SeekTo(position)));

    public static VarAffine2x3 ReadFrom(Stream stream) => new()
    {
        XX = stream.ReadFixed(),
        YX = stream.ReadFixed(),
        XY = stream.ReadFixed(),
        YY = stream.ReadFixed(),
        DX = stream.ReadFixed(),
        DY = stream.ReadFixed(),
        VarIndexBase = stream.ReadUIntByBigEndian(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteFixed(XX);
        stream.WriteFixed(YX);
        stream.WriteFixed(XY);
        stream.WriteFixed(YY);
        stream.WriteFixed(DX);
        stream.WriteFixed(DY);
        stream.WriteUIntByBigEndian(VarIndexBase);
    }

    public int SizeOf() => XX.SizeOf() + YX.SizeOf() +
        XY.SizeOf() + YY.SizeOf() +
        DX.SizeOf() + DY.SizeOf() +
        VarIndexBase.SizeOf();
}
