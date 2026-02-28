using System;

namespace OpenType.Tables;

public interface ICMapFormat : IExportable
{
    public ushort Format { get; init; }

    public Func<int, uint> CreateCharToGID();
}
