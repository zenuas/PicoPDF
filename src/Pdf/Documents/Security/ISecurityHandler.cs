namespace PicoPDF.Pdf.Documents.Security;

public interface ISecurityHandler
{
    public byte[] Key { get; init; }

    public IFilter CreateEncrypter(int object_number, int generation_number);
    public IFilter CreateDecrypter(int object_number, int generation_number);
}
