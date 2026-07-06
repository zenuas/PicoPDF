using System.IO;

namespace OpenType;

public class FontPath : IFontPath
{
    public required string Path { get; init; }

    public Stream Open() => File.Open(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
    public string GetPath() => System.IO.Path.GetFullPath(Path);
}
