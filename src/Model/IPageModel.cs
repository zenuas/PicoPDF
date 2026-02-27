namespace PicoPDF.Model;

public interface IPageModel
{
    public int Width { get; init; }
    public int Height { get; init; }

    public abstract static IPageModel CreatePageModel<M>(int width, int height, M[] models)
        where M : ISectionModel;
}
