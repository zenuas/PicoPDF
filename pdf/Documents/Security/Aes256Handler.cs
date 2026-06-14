using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pdf.Documents.Security;

public class Aes256Handler : ISecurityHandler
{
    public required byte[] Key { get; init; }

    public (PipeWriter Input, PipeReader Output) CreateEncrypterPipe(int object_number, int generation_number)
    {
        var input = new Pipe();
        var output = new Pipe();

        _ = Task.Run(async () =>
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            SetEncryptionKey_Algorithm1A(Key, aes);

            var iv_buffer = output.Writer.GetMemory(16);
            aes.IV.CopyTo(iv_buffer);
            output.Writer.Advance(16);
            var iv_result = await output.Writer.FlushAsync();
            if (iv_result.IsCanceled) throw new OperationCanceledException();
            if (iv_result.IsCompleted) throw new InvalidOperationException();

            using var reader = input.Reader.AsStream(leaveOpen: true);
            using var encryptor = aes.CreateEncryptor();
            using var crypto = new CryptoStream(reader, encryptor, CryptoStreamMode.Read);
            try
            {
                while (true)
                {
                    var buffer = output.Writer.GetMemory(4096);
                    var readed = await crypto.ReadAsync(buffer);
                    if (readed == 0) break;

                    output.Writer.Advance(readed);
                    var result = await output.Writer.FlushAsync();
                    if (result.IsCanceled) throw new OperationCanceledException();
                    if (result.IsCompleted) break;
                }
                await output.Writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                await input.Reader.CompleteAsync(ex);
                await output.Writer.CompleteAsync(ex);
            }
            finally
            {
                await input.Reader.CompleteAsync();
            }
        });
        return (input.Writer, output.Reader);
    }

    public (PipeWriter Input, PipeReader Output) CreateDecrypterPipe(int object_number, int generation_number)
    {
        var input = new Pipe();
        var output = new Pipe();

        _ = Task.Run(async () =>
        {
            using var aes = Aes.Create();
            SetEncryptionKey_Algorithm1A(Key, aes);
            var iv_result = await input.Reader.ReadAtLeastAsync(16);
            aes.IV = iv_result.Buffer.Slice(0, 16).ToArray();
            input.Reader.AdvanceTo(iv_result.Buffer.GetPosition(16));

            using var reader = input.Reader.AsStream(leaveOpen: true);
            using var decryptor = aes.CreateDecryptor();
            using var crypto = new CryptoStream(reader, decryptor, CryptoStreamMode.Read);
            try
            {
                while (true)
                {
                    var buffer = output.Writer.GetMemory(4096);
                    var readed = await crypto.ReadAsync(buffer);
                    if (readed == 0) break;

                    output.Writer.Advance(readed);
                    var result = await output.Writer.FlushAsync();
                    if (result.IsCanceled) throw new OperationCanceledException();
                    if (result.IsCompleted) break;
                }
                await output.Writer.CompleteAsync();
            }
            catch (Exception ex)
            {
                await input.Reader.CompleteAsync(ex);
                await output.Writer.CompleteAsync(ex);
            }
            finally
            {
                await input.Reader.CompleteAsync();
            }
        });
        return (input.Writer, output.Reader);
    }

    public IConverter CreateEncrypterConverter(int object_number, int generation_number)
    {
        var aes = Aes.Create();
        try
        {
            SetEncryptionKey_Algorithm1A(Key, aes);
            return new ConverterBinder()
            {
                Convert = bytes =>
                {
                    aes.GenerateIV();
                    return [.. aes.IV, .. aes.EncryptCbc(bytes, aes.IV)];
                },
                Dispose = () => aes.Dispose(),
            };
        }
        catch
        {
            aes.Dispose();
            throw;
        }
    }

    public IConverter CreateDecrypterConverter(int object_number, int generation_number)
    {
        var aes = Aes.Create();
        try
        {
            SetEncryptionKey_Algorithm1A(Key, aes);
            return new ConverterBinder()
            {
                Convert = bytes => aes.DecryptCbc(bytes[16..], bytes[0..16]),
                Dispose = () => aes.Dispose(),
            };
        }
        catch
        {
            aes.Dispose();
            throw;
        }
    }

    public static byte[] CreateFileEncryptionKey() =>
        // Use the 32-byte file encryption key for the AES-256 symmetric key algorithm, along with the string or stream data to be encrypted.
        RandomNumberGenerator.GetBytes(32);

    public static void SetEncryptionKey_Algorithm1A(ReadOnlySpan<byte> key, Aes aes)
    {
        // Use the AES algorithm in Cipher Block Chaining (CBC) mode, which requires an initialization vector.
        // The block size parameter is set to 16 bytes, and the initialization vector is a 16-byte random number that is stored as the first 16 bytes of the encrypted stream or string.
        aes.BlockSize = 16 * 8;
        aes.Mode = CipherMode.CBC;
        aes.SetKey(key);
    }

    public static void ComputingHash_Algorithm2B(ReadOnlySpan<byte> input, ReadOnlySpan<byte> salt, ReadOnlySpan<byte> user_key, Span<byte> desitination)
    {
        // Take the SHA-256 hash of the original input to the algorithm and name the resulting 32 bytes, K.
        // Perform the following steps (a)-(d) 64 times:
        Span<byte> k = stackalloc byte[512 / 8];
        var k_length = SHA256.HashData([.. input, .. salt, .. user_key], k);

        using var aes = Aes.Create();

        var k0_max_length = input.Length + k.Length + user_key.Length;
        Span<byte> k1 = stackalloc byte[k0_max_length * 64];
        Span<byte> e = stackalloc byte[k0_max_length * 64];
        for (var i = 0; ;)
        {
            // a) Make a new string, K1, consisting of 64 repetitions of the sequence: input password, K, the 48-byte user key.
            var k1_length = input.Length + k_length + user_key.Length;

            input.CopyTo(k1);
            k[..k_length].CopyTo(k1[input.Length..]);
            // The 48 byte user key is only used when checking the owner password or creating the owner key.
            // If checking the user password or creating the user key, K1 is the concatenation of the input password and K.
            if (user_key.Length > 0) user_key.CopyTo(k1[(input.Length + k_length)..]);
            for (var j = 1; j < 64; j++) k1[..k1_length].CopyTo(k1[(k1_length * j)..]);

            // b) Encrypt K1 with the AES-128 (CBC, no padding) algorithm, using the first 16 bytes of K as the key and the second 16 bytes of K as the initialization vector.
            // The result of this encryption is E.
            aes.SetKey(k[..16]);
            var e_length = aes.EncryptCbc(k1[..(k1_length * 64)], k[16..32], e, PaddingMode.None);

            // c) Taking the first 16 bytes of E as an unsigned big-endian integer, compute the remainder, modulo 3.
            // If the result is 0, the next hash used is SHA-256,
            // if the result is 1, the next hash used is SHA-384,
            // if the result is 2, the next hash used is SHA-512.
            UInt128 evalue = 0;
            for (var j = 0; j < 16; j++) evalue = (evalue << 8) | e[j];
            k_length = (int)(evalue % 3) switch
            {
                // d) Using the hash algorithm determined in step c, take the hash of E.
                // The result is a new value of K, which will be 32, 48, or 64 bytes in length.
                0 => SHA256.HashData(e[..e_length], k),
                1 => SHA384.HashData(e[..e_length], k),
                2 => SHA512.HashData(e[..e_length], k),
                _ => throw new(),
            };

            // e) Look at the very last byte of E.
            // If the value of that byte (taken as an unsigned integer) is greater than the round number - 32, repeat steps (a-d) again.
            if (++i > 63 && e[e_length - 1] <= i - 32) break;
        }
        // f) Repeat from steps (a-e) until the value of the last byte is ≤ (round number) - 32.
        k[..32].CopyTo(desitination);
    }

    public static void ComputeUserPassword_Algorithm8(ReadOnlySpan<byte> user_password, ReadOnlySpan<byte> file_encryption_key, Span<byte> u_key, Span<byte> ue_key)
    {
        // Generate 16 random bytes of data using a strong random number generator.
        // The first 8 bytes are the User Validation Salt.
        // The second 8 bytes are the User Key Salt.
        // Compute the 32-byte hash using algorithm 2.B with an input string consisting of the UTF-8 password concatenated with the User Validation Salt.
        // The 48- byte string consisting of the 32-byte hash followed by the User Validation Salt followed by the User Key Salt is stored as the U key.
        var user_validation_salt_and_key_salt = RandomNumberGenerator.GetBytes(16);
        Span<byte> user_validation_salt_hash = stackalloc byte[32];
        ComputingHash_Algorithm2B(user_password, user_validation_salt_and_key_salt.AsSpan(0, 8), [], user_validation_salt_hash);
        user_validation_salt_hash.CopyTo(u_key);
        user_validation_salt_and_key_salt.CopyTo(u_key[32..]);

        // Compute the 32-byte hash using algorithm 2.B with an input string consisting of the UTF-8 password concatenated with the User Key Salt.
        // Using this hash as the key, encrypt the file encryption key using AES-256 in CBC mode with no padding and an initialization vector of zero. The resulting 32-byte string is stored as the UE key.
        using var aes = Aes.Create();

        Span<byte> user_key_salt_hash = stackalloc byte[32];
        ComputingHash_Algorithm2B(user_password, user_validation_salt_and_key_salt.AsSpan(8), [], user_key_salt_hash);
        aes.SetKey(user_key_salt_hash);

        _ = aes.EncryptCbc(file_encryption_key, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], ue_key, PaddingMode.None);
    }

    public static void ComputeOwnerPassword_Algorithm9(ReadOnlySpan<byte> owner_password, ReadOnlySpan<byte> file_encryption_key, ReadOnlySpan<byte> u_key, Span<byte> o_key, Span<byte> oe_key)
    {
        // Generate 16 random bytes of data using a strong random number generator.
        // The first 8 bytes are the Owner Validation Salt.
        // The second 8 bytes are the Owner Key Salt.
        // Compute the 32-byte hash using algorithm 2.B with an input string consisting of the UTF-8 password concatenated with the Owner Validation Salt and then concatenated with the 48-byte U string as generated in Algorithm 8.
        // The 48-byte string consisting of the 32-byte hash followed by the Owner Validation Salt followed by the Owner Key Salt is stored as the O key.
        var owner_validation_salt_and_key_salt = RandomNumberGenerator.GetBytes(16);
        Span<byte> owner_validation_salt_hash = stackalloc byte[32];
        ComputingHash_Algorithm2B(owner_password, owner_validation_salt_and_key_salt.AsSpan(0, 8), u_key, owner_validation_salt_hash);
        owner_validation_salt_hash.CopyTo(o_key);
        owner_validation_salt_and_key_salt.CopyTo(o_key[32..]);

        // Compute the 32-byte hash using 7.6.4.3.4, "Algorithm 2.B: Computing a hash (revision 6 and later)" with an input string consisting of the UTF-8 password concatenated with the Owner Key Salt and then concatenated with the 48-byte U string as generated in 7.6.4.4.7, "Algorithm 8: Computing the encryption dictionary’s U (user password) and UE (user encryption) values (Security handlers of revision 6)".
        // Using this hash as the key, encrypt the file encryption key using AES-256 in CBC mode with no padding and an initialization vector of zero.
        // The resulting 32-byte string is stored as the OE key.
        using var aes = Aes.Create();

        Span<byte> owner_key_salt_hash = stackalloc byte[32];
        ComputingHash_Algorithm2B(owner_password, owner_validation_salt_and_key_salt.AsSpan(8), u_key, owner_key_salt_hash);
        aes.SetKey(owner_key_salt_hash);

        _ = aes.EncryptCbc(file_encryption_key, [0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00], oe_key, PaddingMode.None);
    }

    public static void ComputePerms_Algorithm10(UserAccessPermissions permissions, bool metadata_encrypted, ReadOnlySpan<byte> file_encryption_key, Span<byte> perms)
    {
        // Extend the permissions (contents of the P integer) to 64 bits by setting the upper 32 bits to all 1’s.
        Span<byte> text = stackalloc byte[16];
        var p = (uint)(permissions | UserAccessPermissions.Default_PDF20);

        text[0] = (byte)p;
        text[1] = (byte)(p >> 8);
        text[2] = (byte)(p >> 16);
        text[3] = (byte)(p >> 24);
        text[4] = 0xFF;
        text[5] = 0xFF;
        text[6] = 0xFF;
        text[7] = 0xFF;

        // Set byte 8 to the ASCII character "T" or "F" according to the EncryptMetadata boolean.
        text[8] = (byte)(metadata_encrypted ? 'T' : 'F');

        // Set bytes 9-11 to the ASCII characters '"a", "d", "b".
        text[9] = (byte)'a';
        text[10] = (byte)'d';
        text[11] = (byte)'b';

        // Set bytes 12-15 to 4 bytes of random data, which will be ignored.
        RandomNumberGenerator.GetBytes(4).CopyTo(text[12..]);

        // Encrypt the 16-byte block using AES-256 in ECB mode, using the file encryption key as the key.
        // The result (16 bytes) is stored as the Perms string, and checked for validity when the file is opened.
        using var aes = Aes.Create();
        aes.SetKey(file_encryption_key);
        _ = aes.EncryptEcb(text, perms, PaddingMode.None);
    }
}
