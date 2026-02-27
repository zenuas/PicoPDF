namespace PicoPDF.Model;

public interface ISectionModel
{
    public int Depth { get; init; }
    public int Top { get; set; }
    public int Left { get; init; }
    public int Height { get; init; }
    public bool IsFooter { get; init; }

    public void UpdatePosition();
}
