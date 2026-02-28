using System.IO;

namespace OpenType.Tables;

public interface IExportable
{
    public void WriteTo(Stream stream);
}