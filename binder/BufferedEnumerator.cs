using System;
using System.Collections.Generic;

namespace Binder;

public class BufferedEnumerator<T>
{
    public required IEnumerator<T> BaseEnumerator { get; init; }
    public List<T> Buffer { get; init; } = [];
    public bool IsLast => !Next(0, out _);

    public bool Next(int count, out T ret)
    {
        ret = default!;
        while (count >= Buffer.Count)
        {
            if (!BaseEnumerator.MoveNext()) return false;
            Buffer.Add(BaseEnumerator.Current);
        }
        ret = Buffer[count];
        return true;
    }

    public T Pop()
    {
        if (Buffer.Count == 0)
        {
            if (!BaseEnumerator.MoveNext()) throw new IndexOutOfRangeException();
            return BaseEnumerator.Current;
        }
        var value = Buffer[0];
        Buffer.RemoveAt(0);
        return value;
    }

    public void PushBack(T item)
    {
        Buffer.Insert(0, item);
    }
}
