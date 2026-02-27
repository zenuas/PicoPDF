using Binder.Data;

namespace PicoPDF.Loader.Section;

public class Section : ISubSection, IParentSection, IBreakKey
{
    public string BreakKey { get; init; } = "";
    public IHeaderSection? Header { get; init; } = null;
    public required ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; } = null;
}
