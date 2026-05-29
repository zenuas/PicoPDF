using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;

namespace PicoPDF.Pdf.Documents.Security;

public static class Encryption
{
    public static readonly byte[] PasswordPadding_Revision4 = [
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
    ];

    public static byte[] CreatePassword_Revision4(byte[] password, byte[] owner, int permissions, byte[] file_id, int size)
    {
        Span<byte> permissions_buffer = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(permissions_buffer, permissions);

        var hash = MD5.HashData([
                .. password.Concat(PasswordPadding_Revision4).Take(32),
                .. owner,
                .. permissions_buffer,
                ..file_id,
                0xFF, 0xFF, 0xFF, 0xFF,
            ]);
        for (var i = 0; i < 50; i++)
        {
            hash = MD5.HashData(hash[0..size]);
        }
        return hash[0..size];
    }

    public static byte[] CreatePassword_Revision6()
    {
        return [];
    }
}
