using System.IO;

namespace PicoPDF.OpenType.Tables;

public interface IExportable
{
    public void WriteTo(Stream stream);
}