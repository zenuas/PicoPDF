using System.IO;

namespace PicoPDF.OpenType.Tables.PostScript;

public class CharsetsExpertSubset : ICharsets
{
    public required byte Format { get; init; }

    public static CharsetsExpertSubset ReadFrom(Stream stream)
    {
        return new()
        {
            Format = 2,
        };
    }
}
