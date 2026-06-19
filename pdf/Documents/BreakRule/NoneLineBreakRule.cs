using System.Collections.Generic;

namespace Pdf.Documents.BreakRule;

public class NoneLineBreakRule : ILineBreakRule
{
    public IReadOnlySet<int> DenyStartChar { get; init; } = new HashSet<int>();
    public IReadOnlySet<int> DenyEndChar { get; init; } = new HashSet<int>();
}
