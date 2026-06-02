using System;
using System.Linq;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public static class Encryption
{
    public static readonly byte[] PasswordPaddingBytes = [
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
    ];

    public static byte[] PadOrTruncatePassword32Bytes(byte[] password) => [.. password.Concat(PasswordPaddingBytes).Take(32)];

    public static byte[] ComputeEncryptionKey_Revision4(
            byte[] user_password,
            byte[] owner_password,
            UserAccessPermissions permissions,
            byte[]? document_id,
            bool metadata_encrypted
        )
    {
        var p = (uint)(UserAccessPermissions.Default | permissions);
        var hash = MD5.HashData([
                ..PadOrTruncatePassword32Bytes(user_password),
                ..PadOrTruncatePassword32Bytes(owner_password),
                (byte)p,
                (byte)(p >> 8),
                (byte)(p >> 16),
                (byte)(p >> 24),
                ..document_id ?? [],
                ..metadata_encrypted ? (byte[])[] : [0xFF, 0xFF, 0xFF, 0xFF],
            ]);
        for (var i = 0; i < 50; i++) hash = MD5.HashData(hash);
        return hash;
    }

    public static byte[] ComputeOwnerPassword_Revision3(byte[] user_password, byte[] owner_password, int size)
    {
        var hash = MD5.HashData(PadOrTruncatePassword32Bytes(owner_password));
        for (var i = 0; i < 50; i++) hash = MD5.HashData(hash[0..size]);

        var owner_key = PadOrTruncatePassword32Bytes(user_password);
        Span<byte> key = stackalloc byte[size];
        for (var i = 0; i < 20; i++)
        {
            for (var j = 0; j < size; j++) key[j] = (byte)(hash[j] ^ i);
            _ = Arcfour.Encrypt(Arcfour.InitializeKey(key), owner_key);
        }
        return owner_key;
    }

    public static byte[] ComputeUserPassword_Revision2(byte[] document_id, byte[] encryption_key)
    {
        var user_key = MD5.HashData([.. PasswordPaddingBytes, .. document_id]);

        Span<byte> key = stackalloc byte[encryption_key.Length];
        for (var i = 0; i < 20; i++)
        {
            for (var j = 0; j < encryption_key.Length; j++) key[j] = (byte)(encryption_key[j] ^ i);
            _ = Arcfour.Encrypt(Arcfour.InitializeKey(key), user_key);
        }
        return [.. user_key, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
    }
}
