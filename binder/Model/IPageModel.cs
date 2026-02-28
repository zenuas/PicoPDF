namespace Binder.Model;

public interface IPageModel<TPage, TSection>
    where TPage : IPageModel<TPage, TSection>
    where TSection : ISectionModel<TSection>
{
    public abstract static TPage CreatePageModel(int width, int height, TSection[] models);
}
