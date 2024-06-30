using System.IO;

namespace PicoPDF.OpenType;

public interface IExportable
{
    public void WriteTo(Stream stream);
}