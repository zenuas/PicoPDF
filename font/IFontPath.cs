using System.IO;

namespace OpenType;

public interface IFontPath
{
    public Stream Open();
    public string GetPath();
}
