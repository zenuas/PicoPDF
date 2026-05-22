namespace Binder.Data;

public interface IFooterSection : ISection
{
    public bool IsFill { get; }
    public bool IsPageBreak { get; }
    public bool IsFooter { get; }
}
