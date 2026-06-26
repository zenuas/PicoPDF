using Pdf.Documents.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pdf.Elements;

public abstract class ElementValue
{
    public abstract string ToElementString(IConverter? converter);

    public static implicit operator ElementValue(bool x) => new ElementBoolean() { Value = x };
    public static implicit operator ElementValue(int x) => new ElementInteger() { Value = x };
    public static implicit operator ElementValue(long x) => new ElementInteger() { Value = x };
    public static implicit operator ElementValue(string x) => new ElementLiteral() { Value = x };
    public static implicit operator ElementValue(DateTime x) => new ElementDate() { Value = x };
    public static implicit operator ElementValue(PdfObject x) => new ElementIndirectObject() { References = x };

    public static implicit operator ElementValue(bool[] xs) => new ElementArray<ElementBoolean>(xs.Select(x => new ElementBoolean() { Value = x }));
    public static implicit operator ElementValue(int[] xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));
    public static implicit operator ElementValue(long[] xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));
    public static implicit operator ElementValue(string[] xs) => new ElementArray<ElementValue>(xs.Select(x => new ElementLiteral() { Value = x }));
    public static implicit operator ElementValue(DateTime[] xs) => new ElementArray<ElementDate>(xs.Select(x => new ElementDate() { Value = x }));
    public static implicit operator ElementValue(ElementIndirectObject[] xs) => new ElementArray<ElementIndirectObject>(xs);

    public static implicit operator ElementValue(List<bool> xs) => new ElementArray<ElementBoolean>(xs.Select(x => new ElementBoolean() { Value = x }));
    public static implicit operator ElementValue(List<int> xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));
    public static implicit operator ElementValue(List<long> xs) => new ElementArray<ElementInteger>(xs.Select(x => new ElementInteger() { Value = x }));
    public static implicit operator ElementValue(List<string> xs) => new ElementArray<ElementValue>(xs.Select(x => new ElementLiteral() { Value = x }));
    public static implicit operator ElementValue(List<DateTime> xs) => new ElementArray<ElementDate>(xs.Select(x => new ElementDate() { Value = x }));
    public static implicit operator ElementValue(List<ElementIndirectObject> xs) => new ElementArray<ElementIndirectObject>(xs);
}
