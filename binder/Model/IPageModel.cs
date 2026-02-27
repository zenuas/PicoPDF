namespace Binder.Model;

public interface IPageModel
{
    public abstract static IPageModel CreatePageModel<M>(int width, int height, M[] models)
        where M : ISectionModel;
}
