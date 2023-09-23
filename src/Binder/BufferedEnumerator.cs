using System;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Binder;

public class BufferedEnumerator<T>
{
    public required IEnumerator<T> BaseEnumerator { get; init; }
    public List<T> Buffer { get; init; } = new();
    public bool IsLast { get => !Next(0, out _); }

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

    public T[] GetRange(int count)
    {
        while (count > Buffer.Count)
        {
            if (!BaseEnumerator.MoveNext()) throw new IndexOutOfRangeException();
            Buffer.Add(BaseEnumerator.Current);
        }
        var xs = Buffer.Take(count).ToArray();
        Buffer.RemoveRange(0, count);
        return xs;
    }
}
