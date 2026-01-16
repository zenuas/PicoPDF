namespace PicoPDF.Binder.Element;

public interface ICrossSectionElement : IElement
{
    public int Width { get; init; }
    public int Height { get; init; }
}
