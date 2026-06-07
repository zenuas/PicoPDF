using System.IO;

namespace Pdf.Operation;

public interface IOperation
{
    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option);
}
