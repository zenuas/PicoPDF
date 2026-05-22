using Mina.Extension;
using PicoPDF.Pdf.Operation;
using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

public partial class Contents : PdfObject
{
    public required Page Page { get; init; }
    public List<IOperation> Operations { get; init; } = [];

    public override void DoExport(PdfExportOption option)
    {
        var writer = GetWriteStream(option.ContentsStreamDeflate);
        Operations.Each(x => x.OperationWrite(Page.Width, Page.Height, writer, option));
        writer.Flush();
    }

    public static IEnumerable<IOperation> EnumOperations(IEnumerable<IOperation> opes)
    {
        foreach (var op in opes)
        {
            yield return op;
            switch (op)
            {
                case DrawOperations opes2:
                    foreach (var x in EnumOperations(opes2.Operations)) yield return x;
                    break;

                case DrawClipping clip:
                    foreach (var x in EnumOperations(clip.Operations)) yield return x;
                    break;

                case DrawPathOperations path:
                    foreach (var x in EnumOperations(path.Operations)) yield return x;
                    break;
            }
        }
    }
}
