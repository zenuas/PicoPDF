namespace Pdf;

public interface IPdfObject : IHaveReferences
{
    public int IndirectIndex { get; set; }
}
