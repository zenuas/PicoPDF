using System.IO;

namespace PicoPDF.OpenType;

public interface IExportable
{
    public long WriteTo(Stream stream);
}