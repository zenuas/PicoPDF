namespace Image;

public interface IImageCanvas : IImage
{
    public IColor[][] Canvas { get; init; }
}
