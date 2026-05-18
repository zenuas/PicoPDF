namespace Binder.Model;

public interface ISectionBaseModel
{
    public int Depth { get; init; }
    public int Top { get; set; }
    public int Height { get; init; }
    public bool IsFooter { get; init; }

    public void UpdatePosition();
}
