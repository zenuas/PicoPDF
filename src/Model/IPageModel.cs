namespace PicoPDF.Model;

public interface IPageModel
{
    public int Width { get; init; }
    public int Height { get; init; }

    public abstract static IPageModel CreatePageModel(int width, int height, ISectionModel[] models);
}
