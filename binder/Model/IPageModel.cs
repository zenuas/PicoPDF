namespace Binder.Model;

public interface IPageModel<T, M>
    where T : IPageModel<T, M>
    where M : ISectionModel<M>
{
    public abstract static T CreatePageModel(int width, int height, M[] models);
}
