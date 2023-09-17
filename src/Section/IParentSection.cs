namespace PicoPDF.Section;

public interface IParentSection
{
    public IHeaderSection? Header { get; init; }
    public ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; }
}
