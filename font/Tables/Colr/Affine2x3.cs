using Mina.Extension;
using OpenType.Extension;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenType.Tables.Colr;

public class Affine2x3 : IExportable, IAffine2x3
{
    public required Fixed XX { get; init; }
    public required Fixed YX { get; init; }
    public required Fixed XY { get; init; }
    public required Fixed YY { get; init; }
    public required Fixed DX { get; init; }
    public required Fixed DY { get; init; }

    public static Affine2x3 ReadFrom(Stream stream, long position, Dictionary<long, IAffine2x3> colorLineCache) => (Affine2x3)(colorLineCache.TryGetValue(position, out var line) ? line : colorLineCache[position] = ReadFrom(stream.SeekTo(position)));

    public static Affine2x3 ReadFrom(Stream stream) => new()
    {
        XX = stream.ReadFixed(),
        YX = stream.ReadFixed(),
        XY = stream.ReadFixed(),
        YY = stream.ReadFixed(),
        DX = stream.ReadFixed(),
        DY = stream.ReadFixed(),
    };

    public void WriteTo(Stream stream)
    {
        stream.WriteFixed(XX);
        stream.WriteFixed(YX);
        stream.WriteFixed(XY);
        stream.WriteFixed(YY);
        stream.WriteFixed(DX);
        stream.WriteFixed(DY);
    }

    public Matrix3x2 ToMatrix3x2() => new(XX, YX, XY, YY, DX, DY);

    public int SizeOf() => XX.SizeOf() + YX.SizeOf() +
        XY.SizeOf() + YY.SizeOf() +
        DX.SizeOf() + DY.SizeOf();
}
