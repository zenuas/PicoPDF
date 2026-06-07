using System.IO.Pipelines;

namespace Pdf.Documents.Security;

public class IdentityHandler : ISecurityHandler
{
    public byte[] Key { get; init; } = [];

    public (PipeWriter Input, PipeReader Output) CreateEncrypterPipe(int object_number, int generation_number)
    {
        var pipe = new Pipe();
        return (pipe.Writer, pipe.Reader);
    }

    public (PipeWriter Input, PipeReader Output) CreateDecrypterPipe(int object_number, int generation_number)
    {
        var pipe = new Pipe();
        return (pipe.Writer, pipe.Reader);
    }

    public IConverter CreateEncrypterConverter(int object_number, int generation_number) => new IdentityConverter();
    public IConverter CreateDecrypterConverter(int object_number, int generation_number) => new IdentityConverter();
}
