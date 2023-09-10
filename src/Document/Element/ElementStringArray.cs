﻿using Extensions;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Document.Element;

public class ElementStringArray : ElementValue
{
    public List<string> Array { get; init; } = new();

    public ElementStringArray(params string[] xs)
    {
        Array.AddRange(xs);
    }

    public ElementStringArray(IEnumerable<string> xs)
    {
        Array.AddRange(xs);
    }

    public override string ToElementString() => $"[ {Array.Select(x => $"{x}").Join(" ")} ]";
}
