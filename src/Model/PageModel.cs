using PicoPDF.Document;
using System.Collections.Generic;

namespace PicoPDF.Model;

public class PageModel
{
    public required PageSize Size { get; init; }
    public required Orientation Orientation { get; init; }
    public required string DefaultFont { get; init; }
    public List<SectionModel> Models { get; init; } = new();
}
