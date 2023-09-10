using System.IO;

namespace PicoPDF.Document.Operation;

public interface IOperation
{
    public void OperationWrite(int width, int height, Stream writer);
}
