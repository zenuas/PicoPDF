using System.IO;

namespace PicoPDF.Pdf.Operation;

public interface IOperation
{
    public void OperationWrite(int width, int height, Stream writer);
}
