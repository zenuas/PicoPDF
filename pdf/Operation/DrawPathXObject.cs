using Mina.Extension;
using Pdf.XObject;
using System.Collections.Generic;
using System.IO;

namespace Pdf.Operation;

public class DrawPathXObject : IPathOperation, IHaveReferences
{
    public required IXObject XObject { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"/{XObject.Name} Do\n");
    }

    public IEnumerable<IHaveReferences> GetReferences()
    {
        if (XObject is IHaveReferences r) yield return r;
    }
}
