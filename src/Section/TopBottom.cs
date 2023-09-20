namespace PicoPDF.Section;

public class TopBottom
{
    public int Top { get; set; } = 0;
    public required int Bottom { get; set; }

    public int Height { get => Bottom - Top; }
}
