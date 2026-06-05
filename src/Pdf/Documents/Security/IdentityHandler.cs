using System;

namespace PicoPDF.Pdf.Documents.Security;

public class IdentityHandler : ISecurityHandler
{
    public byte[] Key { get; init; } = [];

    public IFilter CreateEncrypter(int object_number, int generation_number) => new IdentityFilter();
    public IFilter CreateDecrypter(int object_number, int generation_number) => new IdentityFilter();

    public Func<ReadOnlySpan<byte>, byte[]> CreateEncrypterFunction(int object_number, int generation_number) => x => [.. x];
    public Func<ReadOnlySpan<byte>, byte[]> CreateDecrypterFunction(int object_number, int generation_number) => x => [.. x];
}
