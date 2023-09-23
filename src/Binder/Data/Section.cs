namespace PicoPDF.Binder.Data;

public class Section : ISubSection, IParentSection
{
    public string BreakKey { get; init; } = "";
    public IHeaderSection? Header { get; init; } = null;
    public required ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; } = null;
}
