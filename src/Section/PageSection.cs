using PicoPDF.Pdf;

namespace PicoPDF.Section;

public class PageSection : IParentSection
{
    public required PageSize Size { get; init; }
    public required Orientation Orientation { get; init; }
    public required string DefaultFont { get; init; }
    public IHeaderSection? Header { get; init; } = null;
    public required ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; } = null;
}
