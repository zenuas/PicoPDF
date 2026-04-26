using Mina.Extension;
using OpenType.Extension;
using OpenType.Outline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace OpenType.Tables.PostScript;

public class CompactFontFormat : IExportable
{
    public required byte Major { get; init; }
    public required byte Minor { get; init; }
    public required byte HeaderSize { get; init; }
    public required byte OffsetSize { get; init; }
    public required string[] Names { get; init; }
    public required TopDict TopDict { get; init; }
    public required string[] Strings { get; init; }
    public required byte[][] GlobalSubroutines { get; init; }

    public static CompactFontFormat ReadFrom(Stream stream)
    {
        var position = stream.Position;

        var major = stream.ReadUByte();
        var minor = stream.ReadUByte();
        var header_size = stream.ReadUByte();
        var offset_size = stream.ReadUByte();
        var names = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var top_dict_data = ReadIndexData(stream).First();
        var strings = ReadIndexData(stream).Select(Encoding.UTF8.GetString).ToArray();
        var global_subr = ReadIndexData(stream);
        var top_dict = TopDict.ReadFrom(top_dict_data, strings, stream, position);

        return new()
        {
            Major = major,
            Minor = minor,
            HeaderSize = header_size,
            OffsetSize = offset_size,
            Names = names,
            TopDict = top_dict,
            Strings = strings,
            GlobalSubroutines = global_subr,
        };
    }

    public static byte[][] ReadIndexData(Stream stream) => ReadIndexDataBody(stream, ReadIndexDataHeader(stream));

    public static int[] ReadIndexDataHeader(Stream stream)
    {
        var count = stream.ReadUShortByBigEndian();
        if (count == 0) return [0];
        var offset_size = stream.ReadUByte();

        Func<Stream, int> offset_read = offset_size switch
        {
            1 => (x) => x.ReadUByte(),
            2 => (x) => x.ReadUShortByBigEndian(),
            3 => (x) => x.Read3BytesByBigEndian(),
            _ => (x) => (int)x.ReadUIntByBigEndian(),
        };

        return [.. Lists.Repeat(() => offset_read(stream)).Take(count + 1)];
    }

    public static byte[][] ReadIndexDataBody(Stream stream, int[] offset) => [.. Enumerable.Range(0, offset.Length - 1).Select(i => stream.ReadExactly(offset[i + 1] - offset[i]))];

    public void WriteTo(Stream stream)
    {
        var position = stream.Position;

        stream.WriteByte(Major);
        stream.WriteByte(Minor);
        stream.WriteByte(HeaderSize);
        stream.WriteByte(OffsetSize);
        WriteIndexData(stream, [.. Names.Select(Encoding.UTF8.GetBytes)]);

        var top_dict_start = stream.Position;
        WriteIndexData(stream, [TopDict.DictDataToBytes(TopDict.Dict)]);
        WriteIndexData(stream, [.. Strings.Select(Encoding.UTF8.GetBytes)]);
        WriteIndexData(stream, GlobalSubroutines);

        TopDict.WriteWithoutDictAndOffsetUpdate(stream, position);

        var lastposition = stream.Position;
        WriteIndexData(stream.SeekTo(top_dict_start), [TopDict.DictDataToBytes(TopDict.Dict)]);
        stream.Position = lastposition;
    }

    public static void WriteIndexData(Stream stream, byte[][] index)
    {
        WriteIndexDataHeader(stream, [.. index.Select(x => (uint)x.Length)]);
        WriteIndexDataBody(stream, index);
    }

    public static void WriteIndexDataHeader(Stream stream, uint[] index)
    {
        stream.WriteUShortByBigEndian((ushort)index.Length);
        if (index.Length == 0) return;

        var offset_max = index.Sum(x => x) + 1;
        var offset_size =
            offset_max <= byte.MaxValue ? 1 :
            offset_max <= ushort.MaxValue ? 2 :
            offset_max <= 0xFFFFFF ? 3 :
            4;
        stream.WriteByte((byte)offset_size);

        Action<Stream, uint> offset_write = offset_size switch
        {
            1 => (x, n) => x.WriteByte((byte)n),
            2 => (x, n) => x.WriteUShortByBigEndian((ushort)n),
            3 => (x, n) => x.Write([(byte)((n >> 16) & 0xFF), (byte)((n >> 8) & 0xFF), (byte)(n & 0xFF)]),
            _ => (x, n) => x.WriteUIntByBigEndian(n),
        };

        var offset = 1U;
        offset_write(stream, 1);
        index.Each(x => offset_write(stream, offset += x));
    }

    public static void WriteIndexDataBody(Stream stream, byte[][] index) => index.Each(x => stream.Write(x));

    public IOutline[] ToOutline(uint gid)
    {
        var private_dict = TopDict.IsCIDFont ?
            TopDict.FontDictArray[gid >= TopDict.FontDictSelect.Length ? (byte)0 : TopDict.FontDictSelect[gid]].PrivateDict :
            TopDict.PrivateDict;
        var local_subr = private_dict?.LocalSubroutines ?? [];
        var surfaces = new List<IEdge[]>();
        var edges = new List<IEdge>();

        var local_bias = Subroutine.GetSubroutineBias(local_subr.Length);
        var global_bias = Subroutine.GetSubroutineBias(GlobalSubroutines.Length);
        Vector2? start_point = null;

        void OperandAction(CharstringCommandCodes ope, List<float> stack, SubroutineFrame frame)
        {
            switch (ope)
            {
                case CharstringCommandCodes.Callsubr:
                    {
                        var index = (int)stack.Pop() + local_bias;
                        Debug.Assert(index >= 0 && index < local_subr.Length);
                        if (index >= 0 && index < local_subr.Length) Subroutine.EnumOperands(local_subr[index], stack, frame, OperandAction);
                        break;
                    }

                case CharstringCommandCodes.Callgsubr:
                    {
                        var index = (int)stack.Pop() + global_bias;
                        Debug.Assert(index >= 0 && index < GlobalSubroutines.Length);
                        if (index >= 0 && index < GlobalSubroutines.Length) Subroutine.EnumOperands(GlobalSubroutines[index], stack, frame, OperandAction);
                        break;
                    }

                case CharstringCommandCodes.Vlineto:
                case CharstringCommandCodes.Hlineto:
                case CharstringCommandCodes.Rlineto:
                case CharstringCommandCodes.Vvcurveto:
                case CharstringCommandCodes.Hhcurveto:
                case CharstringCommandCodes.Vhcurveto:
                case CharstringCommandCodes.Hvcurveto:
                case CharstringCommandCodes.Rlinecurve:
                case CharstringCommandCodes.Rrcurveto:
                case CharstringCommandCodes.Rcurveline:
                    start_point ??= frame.CurrentPoint;
                    Subroutine.DefaultOperandAction(ope, stack, frame);
                    break;

                case CharstringCommandCodes.Vmoveto:
                case CharstringCommandCodes.Hmoveto:
                case CharstringCommandCodes.Rmoveto:
                case CharstringCommandCodes.Endchar:
                    if (start_point is { } s && s != frame.CurrentPoint) frame.AddLine([frame.CurrentPoint, s]);
                    start_point = null;
                    // Every character path and subpath must begin with one of the moveto operators.
                    // If the current path is open when a moveto operator is encountered, the path is closed before performing the moveto operation.
                    Subroutine.DefaultOperandAction(ope, stack, frame);
                    if (edges.Count > 0) surfaces.Add([.. edges]);
                    edges.Clear();
                    break;

                case CharstringCommandCodes.Width:
                    frame.Width ??= stack.Count == 0 ? private_dict?.DefaultWidthX ?? 0 : (int)stack.Pop() + private_dict?.NominalWidthX ?? 0;
                    break;

                default:
                    Subroutine.DefaultOperandAction(ope, stack, frame);
                    break;
            }
        }

        var frame = new SubroutineFrame()
        {
            AddLine = vecs => edges.Add(vecs.Length == 2 ?
                new Line { Start = vecs[0], End = vecs[1] } :
                new BezierCurve { Start = vecs[0], ControlPoint = [vecs[1], vecs[2]], End = vecs[3], ComplementPoint = false }),
        };
        Subroutine.EnumOperands(TopDict.CharStrings[gid < TopDict.CharStrings.Length ? gid : 0], [], frame, OperandAction);
        if (edges.Count > 0) surfaces.Add([.. edges]);
        var minmax = surfaces.Flatten().Select(x => (MinX: x.MinX(), MinY: x.MinY(), MaxX: x.MaxX(), MaxY: x.MaxY())).ToArray();
        var xmin = minmax.Length == 0 ? 0 : minmax.Select(x => x.MinX).Min();
        var ymin = minmax.Length == 0 ? 0 : minmax.Select(x => x.MinY).Min();
        var xmax = minmax.Length == 0 ? 0 : minmax.Select(x => x.MaxX).Max();
        var ymax = minmax.Length == 0 ? 0 : minmax.Select(x => x.MaxY).Max();
        return [.. surfaces.Where(x => x.Length > 0).Select(x => new Surface() { XMin = xmin, YMin = ymin, XMax = xmax, YMax = ymax, Edges = x })];
    }
}
