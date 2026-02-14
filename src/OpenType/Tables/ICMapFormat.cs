using System;

namespace PicoPDF.OpenType.Tables;

public interface ICMapFormat : IExportable
{
    public ushort Format { get; init; }

    public Func<int, uint> CreateCharToGID();
}
