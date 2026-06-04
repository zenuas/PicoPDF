using Mina.Extension;
using PicoPDF.Pdf.Documents.Security;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Elements;

public class ElementDictionary : ElementValue
{
    public Dictionary<string, ElementValue> Dictionary { get; init; } = [];

    public override string ToElementString(int object_number, int generation_number, ISecurityHandler? handler) => $"<< {Dictionary.Select(x => $"/{x.Key} {x.Value.ToElementString(object_number, generation_number, handler)}").Join(" ")} >>";
}
