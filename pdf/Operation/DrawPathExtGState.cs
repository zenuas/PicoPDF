using Mina.Extension;
using Pdf.ExtGState;
using System.IO;

namespace Pdf.Operation;

public class DrawPathExtGState : IPathOperation
{
    public required IGraphicsStateParameter ExtGState { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"/{ExtGState.Name} gs\n");
    }
}
