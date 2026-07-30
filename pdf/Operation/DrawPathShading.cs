using Mina.Extension;
using Pdf.Shading;
using System.Collections.Generic;
using System.IO;

namespace Pdf.Operation;

public class DrawPathShading : IPathOperation, IHaveReferences
{
    public required IShading Shading { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"/{Shading.Name} sh\n");
    }

    public IEnumerable<IHaveReferences> GetReferences()
    {
        if (Shading is IHaveReferences r) yield return r;
    }
}
