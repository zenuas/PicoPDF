using Binder.Data;
using PicoPDF.Model;
using System.Globalization;

namespace PicoPDF.Loader.Sections;

public class PageSection : IPageSection<SectionModel>
{
    public required PageSize Size { get; init; }
    public required Orientations Orientation { get; init; }
    public required FontPath[] DefaultFont { get; init; }
    public IHeaderSection? Header { get; init; } = null;
    public required ISubSection SubSection { get; init; }
    public IFooterSection? Footer { get; init; } = null;
    public AllSides Padding { get; init; } = new(0, 0, 0, 0);
    public CultureInfo DefaultCulture { get; init; } = CultureInfo.InvariantCulture;
    public int Width => Size.GetPageSize(Orientation).Width;
    public int Height => Size.GetPageSize(Orientation).Height;
    public required PdfEventOption EventOption { get; init; }
}
