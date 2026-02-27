namespace Binder.Data;

public interface IFooterSection : ISection
{
    public bool PageBreak { get; init; }
    public bool IsFooter { get; init; }
}
