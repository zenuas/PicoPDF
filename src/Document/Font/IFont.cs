using System.Collections.Generic;

namespace PicoPDF.Document.Font;

public interface IFont : IPdfObject
{
    public string Name { get; init; }
    public IEnumerable<byte> CreateTextShowingOperator(string s);
}
