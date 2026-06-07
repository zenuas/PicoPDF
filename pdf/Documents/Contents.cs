using Mina.Extension;
using Pdf.Operation;
using System.Collections.Generic;

namespace Pdf.Documents;

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
                case IHaveOperations xs:
                    foreach (var x in EnumOperations(xs.Operations)) yield return x;
                    break;

                case IHavePathOperations xs:
                    foreach (var x in EnumOperations(xs.Operations)) yield return x;
                    break;
            }
        }
    }
}
