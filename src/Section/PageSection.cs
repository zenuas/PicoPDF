using PicoPDF.Document;

namespace PicoPDF.Section;

public class PageSection
{
    public required PageSize Size { get; init; }
    public required Orientation Orientation { get; init; }
    public IHeaderSection? Header { get; init; } = null;
    public required ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; } = null;
}
