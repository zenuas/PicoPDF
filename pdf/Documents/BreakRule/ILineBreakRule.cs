using System.Collections.Generic;

namespace Pdf.Documents.BreakRule;

public interface ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; }
    public IReadOnlySet<int> DenyEndChar { get; init; }
}
