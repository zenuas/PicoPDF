namespace PicoPDF.Model;

public class PageModel
{
    public required int Width { get; init; }
    public required int Height { get; init; }
    public SectionModel[] Models { get; init; } = [];
}
