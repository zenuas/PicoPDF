namespace Binder.Data;

public interface IFooterSection : ISection
{
    public bool Fill { get; init; }
    public bool IsPageBreak { get; init; }
    public bool IsFooter { get; init; }
}
