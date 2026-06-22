using System;
using System.IO;

namespace OpenType.Tables;

public record class ScalableVectorGraphicsTable : IExportable
{
    public static ScalableVectorGraphicsTable ReadFrom(Stream stream)
    {
        return new();
    }

    public void WriteTo(Stream stream)
    {
        throw new NotImplementedException();
    }
}
