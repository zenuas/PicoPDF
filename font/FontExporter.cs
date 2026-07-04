using Mina.Extension;
using OpenType.Extension;
using OpenType.Tables;
using OpenType.Tables.PostScript;
using OpenType.Tables.TrueType;
using Svg.Extension;
using Svg.Outline;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenType;

public static class FontExporter
{
    public static byte[] Export(IOpenTypeFont font, FontTypes fonttypes)
    {
        using var stream = new MemoryStream();
        Export(font, fonttypes, stream);
        return stream.ToArray();
    }

    public static void Export(IOpenTypeFont font, FontTypes fonttypes, Stream stream, long start_stream_position = 0)
    {
        var table_names = new string[] { "cmap", "head", "hhea", "hmtx", "maxp", "name", "post" }
            .If(_ => fonttypes == FontTypes.PostScript, xs => xs.Concat("CFF "), xs => xs)
            .If(_ => font.Color is { }, xs => xs.Concat("COLR"), xs => xs)
            .If(_ => font.ColorPalette is { }, xs => xs.Concat("CPAL"), xs => xs)
            .If(_ => font.OS2 is { }, xs => xs.Concat("OS/2"), xs => xs)
            .If(_ => fonttypes == FontTypes.TrueType, xs => xs.Concat(["glyf", "loca"]), xs => xs)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var tables = table_names.ToDictionary(x => x, _ => new MutableTableRecord { Position = 0, Checksum = 0, Offset = 0, Length = 0 });

        var tables_pow = (int)Math.Pow(2, Math.Floor(Math.Log2(tables.Count)));
        new OffsetTable
        {
            SfntVersion = font.Offset.SfntVersion,
            NumberOfTables = (ushort)tables.Count,
            SearchRange = (ushort)(tables_pow * 16),
            EntrySelector = (ushort)Math.Log2(tables_pow),
            RangeShift = (ushort)((tables.Count * 16) - (tables_pow * 16)),
        }.WriteTo(stream);

        foreach (var x in table_names)
        {
            var table = tables[x];
            stream.WriteTag(x);
            table.Position = stream.Position;
            WriteTableRecord(stream, table);
        }

        if (font.Color is { }) ExportTable(stream, start_stream_position, tables["COLR"], font.Color);
        if (font.ColorPalette is { }) ExportTable(stream, start_stream_position, tables["CPAL"], font.ColorPalette);
        if (font.OS2 is { }) ExportTable(stream, start_stream_position, tables["OS/2"], font.OS2);
        ExportTable(stream, start_stream_position, tables["cmap"], font.CMap);
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0));
        ExportTable(stream, start_stream_position, tables["hhea"], font.HorizontalHeader);
        ExportTable(stream, start_stream_position, tables["hmtx"], font.HorizontalMetrics);
        ExportTable(stream, start_stream_position, tables["maxp"], font.MaximumProfile);
        ExportTable(stream, start_stream_position, tables["name"], font.Name);
        ExportTable(stream, start_stream_position, tables["post"], font.PostScript);

        if (font.OS2 is { }) Debug.Assert(tables["OS/2"].Length is 78 or 86 or 96 or 100);
        Debug.Assert(tables["head"].Length == 54);
        Debug.Assert(tables["hhea"].Length == 36);
        Debug.Assert(tables["maxp"].Length is 6 or 32);
        Debug.Assert(tables["post"].Length == 32);

        switch (fonttypes)
        {
            case FontTypes.TrueType: ExportToTrueType(font, tables, stream, start_stream_position); break;
            case FontTypes.PostScript: ExportToPostScript(font, tables, stream, start_stream_position); break;
            default: throw new();
        }

        var lastposition = stream.Position;
        tables.Values.Each(x => MovePositonAndWriteTableRecord(stream, x));
        var checksum = CalcChecksum(stream.SeekTo(start_stream_position), (uint)(lastposition - start_stream_position));
        ExportTable(stream, start_stream_position, tables["head"], x => font.FontHeader.WriteTo(x, 0xB1B0AFBA - checksum));
        stream.Position = lastposition;
    }

    public static void ExportToTrueType(IOpenTypeFont font, Dictionary<string, MutableTableRecord> tables, Stream stream, long start_stream_position = 0)
    {
        var glyphs = Lists.RangeTo(1, font.MaximumProfile.NumberOfGlyphs - 1)
            .Select(x => OutlineToSimpleGlyph(font.GIDToOutline((uint)x, false)))
            .Prepend(new NotdefGlyph())
            .ToArray();

        // glyf table export
        var glyf_start = stream.Position;
        var glyph_index = new Dictionary<int, long>();
        tables["glyf"].Offset = (uint)(glyf_start - start_stream_position);
        var index_to_locformat = font.FontHeader.IndexToLocFormat;
        var lastglyph = glyphs
            .Select((x, i) => (Glyph: x, Index: i))
            .Aggregate(0L, (acc, x) =>
            {
                glyph_index[x.Index] = acc;
                var position = stream.Position;
                x.Glyph.WriteTo(stream);
                return acc + (stream.Position - position) + (index_to_locformat == 0 ? StreamAlignment(stream, 2) : 0);
            });
        tables["glyf"].Length = (uint)(stream.Position - glyf_start);
        _ = StreamAlignment(stream);

        // loca table export
        var loca_start = stream.Position;
        tables["loca"].Offset = (uint)(loca_start - start_stream_position);
        if (index_to_locformat == 0)
        {
            glyphs.Each((_, i) => stream.WriteOffset16((ushort)(glyph_index[i] / 2)));
            stream.WriteOffset16((ushort)(lastglyph / 2));
        }
        else
        {
            glyphs.Each((_, i) => stream.WriteOffset32((uint)glyph_index[i]));
            stream.WriteOffset32((uint)lastglyph);
        }
        tables["loca"].Length = (uint)(stream.Position - loca_start);
        _ = StreamAlignment(stream);
    }

    public static IGlyph OutlineToSimpleGlyph(IOutline[] outlines)
    {
        var surfaces = outlines.OfType<Surface>().ToArray();
        if (surfaces.Length == 0)
        {
            return new NotdefGlyph();
        }
        var end_points_of_contours = new List<ushort>();
        var flags = new List<SimpleGlyphFlags>();
        var xcoordinates = new List<short>();
        var ycoordinates = new List<short>();

        foreach (var surface in surfaces.Where(x => x.Edges.Length > 0))
        {
            var start = surface.Edges.First().Start;
            flags.Add(SimpleGlyphFlags.ON_CURVE_POINT);
            xcoordinates.Add((short)start.X);
            ycoordinates.Add((short)start.Y);

            foreach (var edge in surface.Edges)
            {
                switch (edge)
                {
                    case Line line:
                        flags.Add(SimpleGlyphFlags.ON_CURVE_POINT);
                        xcoordinates.Add((short)line.End.X);
                        ycoordinates.Add((short)line.End.Y);
                        break;

                    case BezierCurve bezier when bezier.ControlPoint.Length == 1:
                        flags.Add(0);
                        xcoordinates.Add((short)bezier.ControlPoint[0].X);
                        ycoordinates.Add((short)bezier.ControlPoint[0].Y);

                        if (!bezier.ComplementPoint)
                        {
                            flags.Add(SimpleGlyphFlags.ON_CURVE_POINT);
                            xcoordinates.Add((short)bezier.End.X);
                            ycoordinates.Add((short)bezier.End.Y);
                        }
                        break;
                }
            }
            end_points_of_contours.Add((ushort)(flags.Count - 1));
        }

        var (width, left, ymax, ymin) = surfaces.GetSurfaceSize();
        return new SimpleGlyph()
        {
            NumberOfContours = (short)end_points_of_contours.Count,
            XMin = (short)left,
            YMin = (short)ymin,
            XMax = (short)(left + width),
            YMax = (short)ymax,
            EndPointsOfContours = [.. end_points_of_contours],
            InstructionLength = 0,
            Instructions = [],
            Flags = [.. flags],
            XCoordinates = RelativeCoordinates(xcoordinates),
            YCoordinates = RelativeCoordinates(ycoordinates),
        };
    }

    public static short[] RelativeCoordinates(List<short> absolute_coordinates)
    {
        if (absolute_coordinates.Count == 0) return [];
        var relative_coordinates = new short[absolute_coordinates.Count];
        relative_coordinates[0] = absolute_coordinates[0];
        for (var i = 1; i < absolute_coordinates.Count; i++)
        {
            relative_coordinates[i] = (short)(absolute_coordinates[i] - absolute_coordinates[i - 1]);
        }
        return relative_coordinates;
    }

    public static void ExportToPostScript(IOpenTypeFont font, Dictionary<string, MutableTableRecord> tables, Stream stream, long start_stream_position = 0)
    {
        var dict = new Dictionary<TopDictOperators, IntOrDouble[]>();
        var strings = new List<string>();
        dict[TopDictOperators.ROS] = [SID.AddSID(strings, "Adobe"), SID.AddSID(strings, "Identity"), 0];
        dict[TopDictOperators.Charset] = [0];
        dict[TopDictOperators.CharStrings] = [0];
        dict[TopDictOperators.FDSelect] = [0];
        dict[TopDictOperators.FDArray] = [0];
        dict[TopDictOperators.CIDCount] = [font.MaximumProfile.NumberOfGlyphs];
        var top_dict = new TopDict
        {
            Strings = [.. strings],
            Dict = dict,
            CharStrings = [.. Lists.RangeTo(0, font.MaximumProfile.NumberOfGlyphs - 1).Select(x => OutlineToCharStrings([.. font.GIDToOutline((uint)x, false).OfType<Surface>()], 0))],
            Charsets = new()
            {
                Format = 0,
                Glyph = [.. Lists.RangeTo(1, font.MaximumProfile.NumberOfGlyphs - 1).Select(x => (ushort)x)],
            },
            FontDictArray = [new TopDict { Dict = [] }],
            FontDictSelect = [.. Lists.Repeat<byte>(0).Take(font.MaximumProfile.NumberOfGlyphs)],
        };

        var cff = new CompactFontFormat
        {
            Major = 1,
            Minor = 0,
            HeaderSize = 4,
            OffsetSize = 3,
            Names = [],
            TopDict = top_dict,
            Strings = [],
            GlobalSubroutines = [],
        };

        ExportTable(stream, start_stream_position, tables["CFF "], cff);
    }

    public static byte[] OutlineToCharStrings(Surface[] surfaces, int nominalWidthX)
    {
        var char_strings = new List<byte>();
        char_strings.AddRange(Interpreter.NumberToBytes(surfaces.GetSurfaceSize().Width - nominalWidthX));

        var current = new Vector2(0, 0);
        foreach (var surface in surfaces.Where(x => x.Edges.Length > 0))
        {
            var start = surface.Edges.First().Start;
            char_strings.AddRange(Interpreter.NumberToBytes(start.X - current.X));
            char_strings.AddRange(Interpreter.NumberToBytes(start.Y - current.Y));
            char_strings.Add((byte)CharstringCommandCodes.Rmoveto);
            current = start;

            foreach (var edge in surface.Edges)
            {
                switch (edge)
                {
                    case Line line:
                        char_strings.AddRange(Interpreter.NumberToBytes(line.End.X - current.X));
                        char_strings.AddRange(Interpreter.NumberToBytes(line.End.Y - current.Y));
                        char_strings.Add((byte)CharstringCommandCodes.Rlineto);
                        current = line.End;
                        break;

                    case BezierCurve bezier when bezier.ControlPoint.Length == 2:
                        {
                            var cp1 = bezier.ControlPoint[0];
                            var cp2 = bezier.ControlPoint[1];
                            char_strings.AddRange(Interpreter.NumberToBytes(cp1.X - current.X));
                            char_strings.AddRange(Interpreter.NumberToBytes(cp1.Y - current.Y));
                            char_strings.AddRange(Interpreter.NumberToBytes(cp2.X - cp1.X));
                            char_strings.AddRange(Interpreter.NumberToBytes(cp2.Y - cp1.Y));
                            char_strings.AddRange(Interpreter.NumberToBytes(bezier.End.X - cp2.X));
                            char_strings.AddRange(Interpreter.NumberToBytes(bezier.End.Y - cp2.Y));
                            char_strings.Add((byte)CharstringCommandCodes.Rrcurveto);
                            current = bezier.End;
                            break;
                        }
                }
            }
        }
        char_strings.Add((byte)CharstringCommandCodes.Endchar);
        return [.. char_strings];
    }

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, IExportable table) => ExportTable(stream, start_stream_position, rec, table.WriteTo);

    public static void ExportTable(Stream stream, long start_stream_position, MutableTableRecord rec, Action<Stream> f)
    {
        rec.Offset = (uint)(stream.Position - start_stream_position);
        Debug.Assert((rec.Offset % 4) == 0);
        var position = stream.Position;
        f(stream);
        rec.Length = (uint)(stream.Position - position);
        _ = StreamAlignment(stream);
    }

    public static void WriteTableRecord(Stream stream, MutableTableRecord table)
    {
        stream.WriteUIntByBigEndian(table.Checksum);
        stream.WriteOffset32(table.Offset);
        stream.WriteUIntByBigEndian(table.Length);
    }

    public static void MovePositonAndWriteTableRecord(Stream stream, MutableTableRecord table)
    {
        Debug.Assert((table.Offset % 4) == 0);
        table.Checksum = CalcChecksum(stream.SeekTo(table.Offset), table.Length);
        WriteTableRecord(stream.SeekTo(table.Position), table);
    }

    public static uint CalcChecksum(Stream stream, uint length)
    {
        var sum = 0U;
        for (var i = 0U; i < length; i += sizeof(uint))
        {
            sum += stream.ReadUIntByBigEndian();
        }
        return sum;
    }

    public static int StreamAlignment(Stream stream, int alignment = 4)
    {
        var padding = (int)(alignment - (stream.Position % alignment));
        if (padding == alignment) return 0;
        stream.Write(Lists.Repeat((byte)0).Take(padding).ToArray());
        return padding;
    }
}

