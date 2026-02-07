using System.Collections.Generic;

namespace PicoPDF.Model;

public class PageModel
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public List<SectionModel> Models { get; init; } = [];
}
