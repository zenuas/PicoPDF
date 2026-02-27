namespace Binder.Data;

public interface ISection
{
    public string Name { get; init; }
    public int Height { get; init; }
    public ViewModes ViewMode { get; init; }
    public IElement[] Elements { get; init; }
}
