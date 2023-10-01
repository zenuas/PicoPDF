namespace PicoPDF.Binder.Data;

public interface IFooterSection : ISection, IHeaderFooter
{
    public bool PageBreak { get; init; }
}
