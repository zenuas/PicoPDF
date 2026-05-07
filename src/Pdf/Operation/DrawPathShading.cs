using Mina.Extension;
using PicoPDF.Pdf.Shading;
using System.IO;

namespace PicoPDF.Pdf.Operation;

public class DrawPathShading : IPathOperation
{
    public required IShading Shading { get; init; }

    public void OperationWrite(int width, int height, Stream writer, PdfExportOption option)
    {
        writer.Write($"/{Shading.Name} sh\n");
    }
}
