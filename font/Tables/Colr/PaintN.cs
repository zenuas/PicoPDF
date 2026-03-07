using Mina.Extension;
using System.IO;

namespace OpenType.Tables.Colr;

public class PaintN : IPaintFormat
{
    public required byte Format { get; init; }

    public void WriteTo(Stream stream)
    {
        stream.WriteByte(Format);
    }

    public int SizeOf() => Format.SizeOf();
}
