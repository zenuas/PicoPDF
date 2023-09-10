using System.Collections.Generic;
using System.Text;

namespace PicoPDF.Document.Font;

public interface IFont : IPdfObject
{
    public string Name { get; init; }
    public Encoding TextEncoding { get; init; }
    public IEnumerable<byte> CreateTextShowingOperator(string s);
}
