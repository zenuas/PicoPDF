using System;

namespace OpenType.Tables.CMap;

public interface ICharToGID
{
    public Func<int, uint> CreateCharToGID();
}
