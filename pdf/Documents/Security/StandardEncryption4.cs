using Pdf.Extension;
using System;
using System.Text;

namespace Pdf.Documents.Security;

public class StandardEncryption4 : PdfObject, IStandardEncryption
{
    public required byte[] EncryptionKey { get; init; }
    public required byte[] DocumentID { get; init; }
    public ISecurityHandler? StreamHandler { get; init; } = null;
    public ISecurityHandler? StringHandler { get; init; } = null;
    public ISecurityHandler? EmbeddedFileStreamsHandler { get; init; } = null;

    public static StandardEncryption4 Create(CFM cfm, string user_password, string owner_password, UserAccessPermissions permissions, byte[] document_id)
    {
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);
        Span<byte> o_key = stackalloc byte[32];
        Aes128Handler.ComputeOwnerPassword_Algorithm3(
              user_password_bytes,
              owner_password_bytes,
            16,
            o_key
        );
        var encryption_key = Aes128Handler.ComputeEncryptionKey_Algorithm2(
            user_password_bytes,
            o_key,
            permissions,
            document_id,
            true
        );
        var handler = cfm == CFM.None ? (ISecurityHandler)new IdentityHandler() : new Aes128Handler() { Key = encryption_key };
        return new()
        {
            EncryptionKey = encryption_key,
            DocumentID = document_id,
            StreamHandler = handler,
            StringHandler = handler,
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default),
                ["V"] = 4,
                ["CF"] = $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen /Length 128 >> >>",
                ["R"] = 4,
                ["O"] = o_key.ToHexString(),
                ["U"] = Aes128Handler.ComputeUserPassword_Algorithm5(document_id, encryption_key).ToHexString(),
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
            }
        };
    }
}
