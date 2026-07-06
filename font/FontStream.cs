using System.IO;

namespace OpenType;

public class FontStream : IFontPath
{
    public required Stream Stream { get; init; }

    public Stream Open() => Stream;
    public string GetPath() => "";
}
