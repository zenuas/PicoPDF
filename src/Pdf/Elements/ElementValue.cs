using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Pdf.Elements;

public abstract class ElementValue
{
    public abstract string ToElementString();

    public static implicit operator ElementValue(int x) => new ElementInteger() { Value = x };

    public static implicit operator ElementValue(long x) => new ElementInteger() { Value = x };

    public static implicit operator ElementValue(string x) => new ElementString() { Value = x };

    public static implicit operator ElementValue(DateTime x) => new ElementDate() { Value = x };

    public static implicit operator ElementValue(PdfObject x) => new ElementIndirectObject() { References = x };

    public static implicit operator ElementValue(int[] xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));

    public static implicit operator ElementValue(long[] xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));

    public static implicit operator ElementValue(List<int> xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));

    public static implicit operator ElementValue(List<long> xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));

    public static implicit operator ElementValue(string[] xs) => new ElementArray<ElementString>(xs.Select(x => new ElementString() { Value = x }));

    public static implicit operator ElementValue(List<string> xs) => new ElementArray<ElementString>(xs.Select(x => new ElementString() { Value = x }));

    public static implicit operator ElementValue(ElementIndirectObject[] xs) => new ElementArray<ElementIndirectObject>(xs);

    public static implicit operator ElementValue(List<ElementIndirectObject> xs) => new ElementArray<ElementIndirectObject>(xs);
}
