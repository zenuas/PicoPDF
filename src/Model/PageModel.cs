using PicoPDF.Pdf;
using System.Collections.Generic;

namespace PicoPDF.Model;

public class PageModel
{
    public required PageSize Size { get; init; }
    public required Orientation Orientation { get; init; }
    public List<SectionModel> Models { get; init; } = [];
}
