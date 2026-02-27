namespace Binder.Data;

public interface ICrossSectionElement : IElement
{
    public int Width { get; init; }
    public int Height { get; init; }
}
