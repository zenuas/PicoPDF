using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Binder;

public class NullMapper<T> : IReadOnlyDictionary<string, Func<T, object>>
{
    public readonly static NullMapper<T> Instance = new();

    public Func<T, object> this[string key] => _ => null!;
    public IEnumerable<string> Keys => [];
    public IEnumerable<Func<T, object>> Values => [];
    public int Count => 0;

    public bool ContainsKey(string key) => true;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<string, Func<T, object>>> GetEnumerator()
    {
        yield break;
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out Func<T, object> value)
    {
        value = null!;
        return true;
    }
}
