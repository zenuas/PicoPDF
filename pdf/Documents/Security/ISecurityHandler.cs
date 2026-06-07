using System.IO.Pipelines;

namespace Pdf.Documents.Security;

public interface ISecurityHandler
{
    public byte[] Key { get; init; }

    public (PipeWriter Input, PipeReader Output) CreateEncrypterPipe(int object_number, int generation_number);
    public (PipeWriter Input, PipeReader Output) CreateDecrypterPipe(int object_number, int generation_number);

    public IConverter CreateEncrypterConverter(int object_number, int generation_number);
    public IConverter CreateDecrypterConverter(int object_number, int generation_number);
}
