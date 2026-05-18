using System.Collections.Generic;

namespace PicoPDF.Pdf.Documents;

public interface ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; }
    public IReadOnlySet<int> DenyEndChar { get; init; }
}
