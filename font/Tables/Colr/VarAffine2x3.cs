using Mina.Extension;
using OpenType.Extension;
using System.IO;

namespace OpenType.Tables;

public class VarAffine2x3 : IExportable
{
    public required uint XX { get; init; }
    public required uint YX { get; init; }
    public required uint XY { get; init; }
    public required uint YY { get; init; }
    public required uint DX { get; init; }
    public required uint DY { get; init; }
    public required uint VarIndexBase { get; init; }

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
