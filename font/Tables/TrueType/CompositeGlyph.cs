using Mina.Extension;
using OpenType.Extension;
using OpenType.Outline;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenType.Tables.TrueType;

public class CompositeGlyph : IGlyph
{
    public required short NumberOfContours { get; init; }
    public required short XMin { get; init; }
    public required short YMin { get; init; }
    public required short XMax { get; init; }
    public required short YMax { get; init; }
    public required ICompositeGlyphRecord[] CompositeGlyphRecords { get; init; }
    public required ushort InstructionLength { get; init; }
    public required byte[] Instructions { get; init; }

    public IOutline[] ToOutline(IReadOnlyList<IGlyph> glyphs)
    {
        var outlines = new List<IOutline>();
        foreach (var composite in CompositeGlyphRecords)
        {
            glyphs[composite.GlyphIndex].ToOutline(glyphs)
                .OfType<Surface>()
                .Select(x => Composite(composite, x, XMin, YMin, XMax, YMax))
                .Each(outlines.Add);
        }
        return [.. outlines];
    }

    public static Surface Composite(ICompositeGlyphRecord composite, Surface surface, float xmin, float ymin, float xmax, float ymax)
    {
        var transform = Matrix3x2.Identity;
        transform.Translation = new Vector2(composite.Argument1, composite.Argument2);

        if (composite is CompositeGlyphScaleRecord scale)
        {
            transform.M11 = transform.M22 = scale.Scale;
        }
        else if (composite is CompositeGlyphXYScaleRecord xyscale)
        {
            transform.M11 = xyscale.XScale;
            transform.M22 = xyscale.YScale;
        }
        else if (composite is CompositeGlyphMatrix2x2Record matrix)
        {
            transform.M11 = matrix.XScale;
            transform.M12 = matrix.Scale01;
            transform.M21 = matrix.Scale10;
            transform.M22 = matrix.YScale;
        }

        return new()
        {
            XMin = xmin,
            YMin = ymin,
            XMax = xmax,
            YMax = ymax,
            Edges = [.. surface.Edges.Select(edge => edge switch
                {
                    Line line => (IEdge)new Line()
                        {
                            Start = Vector2.Transform(line.Start, transform),
                            End = Vector2.Transform(line.End, transform),
                        },
                    BezierCurves bezier => new BezierCurves()
                        {
                            Start = Vector2.Transform(bezier.Start, transform),
                            End = Vector2.Transform(bezier.End, transform),
                            ControlPoint = [.. bezier.ControlPoint.Select(x => Vector2.Transform(x, transform))],
                            ComplementPoint = bezier.ComplementPoint,
                        },
                    _ => throw new(),
                })],
        };
    }

    public static CompositeGlyph ReadFrom(Stream stream, short number_of_contours)
    {
        var xmin = stream.ReadShortByBigEndian();
        var ymin = stream.ReadShortByBigEndian();
        var xmax = stream.ReadShortByBigEndian();
        var ymax = stream.ReadShortByBigEndian();

        var records = new List<ICompositeGlyphRecord>();
        ushort instruction_length = 0;
        byte[]? instructions = null;
        while (true)
        {
            var flags = (CompositeGlyphFlags)stream.ReadUShortByBigEndian();
            var glyph_index = stream.ReadUShortByBigEndian();
            int arg1;
            int arg2;

            var isargs_are_xy_values = flags.HasFlag(CompositeGlyphFlags.ARGS_ARE_XY_VALUES);
            if (flags.HasFlag(CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
            {
                if (isargs_are_xy_values)
                {
                    arg1 = stream.ReadShortByBigEndian();
                    arg2 = stream.ReadShortByBigEndian();
                }
                else
                {
                    arg1 = stream.ReadUShortByBigEndian();
                    arg2 = stream.ReadUShortByBigEndian();
                }
            }
            else
            {
                if (isargs_are_xy_values)
                {
                    arg1 = stream.ReadSByte();
                    arg2 = stream.ReadSByte();
                }
                else
                {
                    arg1 = stream.ReadUByte();
                    arg2 = stream.ReadUByte();
                }
            }

            if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_SCALE))
            {
                records.Add(new CompositeGlyphScaleRecord()
                {
                    Flags = flags,
                    GlyphIndex = glyph_index,
                    Argument1 = arg1,
                    Argument2 = arg2,
                    Scale = stream.ReadF2DOT14(),
                });
            }
            else if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_AN_X_AND_Y_SCALE))
            {
                records.Add(new CompositeGlyphXYScaleRecord()
                {
                    Flags = flags,
                    GlyphIndex = glyph_index,
                    Argument1 = arg1,
                    Argument2 = arg2,
                    XScale = stream.ReadF2DOT14(),
                    YScale = stream.ReadF2DOT14(),
                });
            }
            else if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_A_TWO_BY_TWO))
            {
                records.Add(new CompositeGlyphMatrix2x2Record()
                {
                    Flags = flags,
                    GlyphIndex = glyph_index,
                    Argument1 = arg1,
                    Argument2 = arg2,
                    XScale = stream.ReadF2DOT14(),
                    Scale01 = stream.ReadF2DOT14(),
                    Scale10 = stream.ReadF2DOT14(),
                    YScale = stream.ReadF2DOT14(),
                });
            }
            else
            {
                records.Add(new CompositeGlyphRecord()
                {
                    Flags = flags,
                    GlyphIndex = glyph_index,
                    Argument1 = arg1,
                    Argument2 = arg2,
                });
            }

            if (!flags.HasFlag(CompositeGlyphFlags.MORE_COMPONENTS))
            {
                if (flags.HasFlag(CompositeGlyphFlags.WE_HAVE_INSTRUCTIONS))
                {
                    instruction_length = stream.ReadUShortByBigEndian();
                    instructions = [.. Lists.Repeat(stream.ReadUByte).Take(instruction_length)];
                }
                break;
            }
        }

        return new()
        {
            NumberOfContours = number_of_contours,
            XMin = xmin,
            YMin = ymin,
            XMax = xmax,
            YMax = ymax,
            CompositeGlyphRecords = [.. records],
            InstructionLength = instruction_length,
            Instructions = instructions ?? [],
        };
    }

    public void WriteTo(Stream stream)
    {
        stream.WriteShortByBigEndian(NumberOfContours);
        stream.WriteShortByBigEndian(XMin);
        stream.WriteShortByBigEndian(YMin);
        stream.WriteShortByBigEndian(XMax);
        stream.WriteShortByBigEndian(YMax);

        foreach (var glyph in CompositeGlyphRecords)
        {
            stream.WriteUShortByBigEndian((ushort)glyph.Flags);
            stream.WriteUShortByBigEndian(glyph.GlyphIndex);

            if (glyph.Flags.HasFlag(CompositeGlyphFlags.ARG_1_AND_2_ARE_WORDS))
            {
                stream.WriteUShortByBigEndian((ushort)glyph.Argument1);
                stream.WriteUShortByBigEndian((ushort)glyph.Argument2);
            }
            else
            {
                stream.WriteByte((byte)glyph.Argument1);
                stream.WriteByte((byte)glyph.Argument2);
            }

            if (glyph is CompositeGlyphScaleRecord scale)
            {
                stream.WriteF2DOT14(scale.Scale);
            }
            else if (glyph is CompositeGlyphXYScaleRecord xyscale)
            {
                stream.WriteF2DOT14(xyscale.XScale);
                stream.WriteF2DOT14(xyscale.YScale);
            }
            else if (glyph is CompositeGlyphMatrix2x2Record matrix)
            {
                stream.WriteF2DOT14(matrix.XScale);
                stream.WriteF2DOT14(matrix.Scale01);
                stream.WriteF2DOT14(matrix.Scale10);
                stream.WriteF2DOT14(matrix.YScale);
            }
        }

        if (Instructions.Length > 0)
        {
            stream.WriteUShortByBigEndian((ushort)Instructions.Length);
            stream.Write(Instructions);
        }
    }
}
