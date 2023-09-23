using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Section;

public class BufferedEnumerator<T> : IEnumerator<T>
{
    public required IEnumerator<T> BaseEnumerator { get; init; }
    public List<T> Buffer { get; init; } = new();
    public bool First { get; set; } = true;
    public bool IsLast { get => Buffer.Count == 0 && !MoveNext(); }
    public T Current => Buffer[0];
    object IEnumerator.Current => Buffer[0]!;

    public void Dispose()
    {
        BaseEnumerator.Dispose();
        Buffer.Clear();
        First = true;
    }

    public bool MoveNext()
    {
        if (!First && Buffer.Count > 0) Buffer.RemoveAt(0);
        First = false;
        return Next(0, out var _);
    }

    public void Reset()
    {
        BaseEnumerator.Reset();
        Buffer.Clear();
        First = true;
    }

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
