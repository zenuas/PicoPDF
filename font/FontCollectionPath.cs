using System.IO;

namespace OpenType;

public class FontCollectionPath : IFontPath
{
    public required string Path { get; init; }
    public required int Index { get; init; }

    public Stream Open() => File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    public string GetPath() => $"{System.IO.Path.GetFullPath(Path)},{Index}";
}
