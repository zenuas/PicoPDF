using System;
using System.IO.Pipelines;
using System.Security.Cryptography;

namespace Pdf.Documents.Security;

public class Aes256Handler : ISecurityHandler
{
    public required byte[] Key { get; init; }

    public static byte[] ComputingHash_Algorithm2B(ReadOnlySpan<byte> input, ReadOnlySpan<byte> salt, byte[]? user_key = null)
    {
        Span<byte> k = stackalloc byte[512 / 8];
        var k_length = SHA256.HashData([.. input, .. salt], k);

        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.None;
        aes.BlockSize = 16 * 8;

        var k0_max_length = input.Length + (512 / 8) + (user_key?.Length ?? 0);
        Span<byte> k1 = stackalloc byte[k0_max_length * 64];
        Span<byte> e = stackalloc byte[k0_max_length * 64];
        for (var i = 0; ;)
        {
            var k1_length = input.Length + k_length + (user_key?.Length ?? 0);

            input.CopyTo(k1);
            k.CopyTo(k1[input.Length..]);
            user_key?.CopyTo(k1[(input.Length + k.Length)..]);
            for (var j = 1; j < 64; j++) k1[..k1_length].CopyTo(k1[(k1_length * j)..]);

            aes.SetKey(k[..16]);
            var e_length = aes.EncryptCbc(k1[..(k1_length * 64)], k[16..32], e, PaddingMode.None);

            UInt128 evalue = 0;
            for (var j = 0; j < 16; j++) evalue = (evalue << 8) | e[j];
            k_length = (int)(evalue % 3) switch
            {
                0 => SHA256.HashData(e[..e_length], k),
                1 => SHA384.HashData(e[..e_length], k),
                2 => SHA512.HashData(e[..e_length], k),
                _ => throw new(),
            };

            if (++i > 63 && e[e_length - 1] <= i - 32) break;
        }
        return k[..32].ToArray();
    }

    public (PipeWriter Input, PipeReader Output) CreateEncrypterPipe(int object_number, int generation_number)
    {
        // Generate 16 random bytes of data using a strong random number generator.
        // The first 8 bytes are the User Validation Salt.
        // The second 8 bytes are the User Key Salt.
        // Compute the 32-byte hash using algorithm 2.B with an input string consisting of the UTF-8 password concatenated with the User Validation Salt.
        // The 48- byte string consisting of the 32-byte hash followed by the User Validation Salt followed by the User Key Salt is stored as the U key.
        var user_validation_salt_and_key_salt = RandomNumberGenerator.GetBytes(16);
        var owner_validation_salt_and_key_salt = RandomNumberGenerator.GetBytes(16);
        var ukey = RandomNumberGenerator.GetBytes(32);

        throw new NotImplementedException();
    }

    public (PipeWriter Input, PipeReader Output) CreateDecrypterPipe(int object_number, int generation_number)
    {
        throw new NotImplementedException();
    }

    public IConverter CreateEncrypterConverter(int object_number, int generation_number)
    {
        throw new NotImplementedException();
    }

    public IConverter CreateDecrypterConverter(int object_number, int generation_number)
    {
        throw new NotImplementedException();
    }
}
