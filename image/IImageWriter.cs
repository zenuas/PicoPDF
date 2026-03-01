using System.IO;

namespace Image;

public interface IImageWritable
{
    public void Write(Stream stream);
}
