namespace PicoPDF.Pdf.Documents.Security;

public class IdentityHandler : ISecurityHandler
{
    public byte[] Key { get; init; } = [];

    public IFilter CreateEncrypter(int object_number, int generation_number) => new IdentityFilter();
    public IFilter CreateDecrypter(int object_number, int generation_number) => new IdentityFilter();

    public IConverter CreateEncrypterConverter(int object_number, int generation_number) => new IdentityConverter();
    public IConverter CreateDecrypterConverter(int object_number, int generation_number) => new IdentityConverter();
}
