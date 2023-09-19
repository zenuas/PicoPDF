using System.Collections.Generic;

namespace PicoPDF.Pdf.Font;

public interface IFont : IPdfObject
{
    public string Name { get; init; }
    public IEnumerable<byte> CreateTextShowingOperator(string s);
}
