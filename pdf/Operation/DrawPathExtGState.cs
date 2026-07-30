using Mina.Extension;
using Pdf.ExtGState;
using System.Collections.Generic;
using System.IO;

namespace Pdf.Operation;

public class DrawPathExtGState : IPathOperation, IHaveReferences
{
    public required IGraphicsStateParameter ExtGState { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"/{ExtGState.Name} gs\n");
    }

    public IEnumerable<IHaveReferences> GetReferences()
    {
        if (ExtGState is IHaveReferences r) yield return r;
    }
}
