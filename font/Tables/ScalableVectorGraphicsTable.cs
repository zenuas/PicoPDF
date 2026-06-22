using System.IO;

namespace OpenType.Tables;

public record class ScalableVectorGraphicsTable
{
    public static ScalableVectorGraphicsTable ReadFrom(Stream stream)
    {
        return new();
    }
}
