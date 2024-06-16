namespace PicoPDF.Binder.Data;

public interface IFooterSection : ISection
{
    public bool PageBreak { get; init; }
}
