namespace Pdf.Documents.Security;

public interface IStandardEncryption
{
    public byte[] EncryptionKey { get; init; }
    public bool MetadataEncrypted { get; init; }
    public ISecurityHandler? StreamHandler { get; init; }
    public ISecurityHandler? StringHandler { get; init; }
    public ISecurityHandler? EmbeddedFileStreamsHandler { get; init; }
}
