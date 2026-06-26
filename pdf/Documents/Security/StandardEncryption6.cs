using Pdf.Extension;
using System;
using System.Text;

namespace Pdf.Documents.Security;

public class StandardEncryption6 : PdfObject, IStandardEncryption
{
    public required byte[] EncryptionKey { get; init; }
    public required bool MetadataEncrypted { get; init; }
    public ISecurityHandler? StreamHandler { get; init; } = null;
    public ISecurityHandler? StringHandler { get; init; } = null;
    public ISecurityHandler? EmbeddedFileStreamsHandler { get; init; } = null;

    public static StandardEncryption6 Create(CFM cfm, string user_password, string owner_password, UserAccessPermissions permissions, bool metadata_encrypted = true)
    {
        // Truncate the UTF-8 representation to 127 bytes if it is longer than 127 bytes.
        var user_password_bytes = Encoding.UTF8.GetBytes(user_password);
        var owner_password_bytes = Encoding.UTF8.GetBytes(owner_password);

        var file_encryption_key = Aes256Handler.CreateFileEncryptionKey();
        Span<byte> u_key = stackalloc byte[48];
        Span<byte> ue_key = stackalloc byte[32];
        Aes256Handler.ComputeUserPassword_Algorithm8(user_password_bytes.Length > 127 ? user_password_bytes[..127] : user_password_bytes, file_encryption_key, u_key, ue_key);

        Span<byte> o_key = stackalloc byte[48];
        Span<byte> oe_key = stackalloc byte[32];
        Aes256Handler.ComputeOwnerPassword_Algorithm9(owner_password_bytes.Length > 127 ? owner_password_bytes[..127] : owner_password_bytes, file_encryption_key, u_key, o_key, oe_key);

        Span<byte> perms = stackalloc byte[16];
        Aes256Handler.ComputePerms_Algorithm10(permissions, metadata_encrypted, file_encryption_key, perms);

        var handler = cfm == CFM.None ? (ISecurityHandler)new IdentityHandler() : new Aes256Handler() { Key = file_encryption_key };
        return new()
        {
            EncryptionKey = file_encryption_key,
            MetadataEncrypted = metadata_encrypted,
            StreamHandler = handler,
            StringHandler = handler,
            Elements =
            {
                ["Filter"] = "/Standard",
                ["P"] = (int)(permissions | UserAccessPermissions.Default_PDF20),
                ["V"] = 5,
                ["CF"] = $"<< /StdCF << /CFM /{cfm} /AuthEvent /DocOpen >> >>",
                ["R"] = 6,
                ["O"] = o_key.ToHexString(),
                ["U"] = u_key.ToHexString(),
                ["OE"] = oe_key.ToHexString(),
                ["UE"] = ue_key.ToHexString(),
                ["Perms"] = perms.ToHexString(),
                ["EncryptMetadata"] = metadata_encrypted,
                ["StmF"] = "/StdCF",
                ["StrF"] = "/StdCF",
                ["Length"] = 256, // NOTE (2020) The Length key was corrected to be required and the descriptive text updated.
            }
        };
    }
}
