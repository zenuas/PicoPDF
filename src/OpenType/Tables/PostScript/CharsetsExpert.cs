using System.IO;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CharsetsExpert : ICharsets
{
    public required byte Format { get; init; }

    public static CharsetsExpert ReadFrom(Stream stream)
    {
        return new()
        {
            Format = 1,
        };
    }
}
