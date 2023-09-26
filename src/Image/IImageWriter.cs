using System.IO;

namespace PicoPDF.Image;

public interface IImageWritable
{
    public void Write(Stream stream);
}
