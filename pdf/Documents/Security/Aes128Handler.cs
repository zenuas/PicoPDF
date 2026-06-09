using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Pdf.Documents.Security;

public class Aes128Handler : ISecurityHandler
{
    public required byte[] Key { get; init; }

    public static readonly byte[] PasswordPaddingBytes = [
        0x28, 0xBF, 0x4E, 0x5E, 0x4E, 0x75, 0x8A, 0x41, 0x64, 0x00, 0x4E, 0x56, 0xFF, 0xFA, 0x01, 0x08,
        0x2E, 0x2E, 0x00, 0xB6, 0xD0, 0x68, 0x3E, 0x80, 0x2F, 0x0C, 0xA9, 0xFE, 0x64, 0x53, 0x69, 0x7A,
    ];

    // If using the AES algorithm, extend the encryption key an additional 4 bytes by adding the value "sAlT", which corresponds to the hexadecimal values 0x73, 0x41, 0x6C, 0x54.
    // (This addition is done for backward compatibility and is not intended to provide additional security.)
    public static readonly byte[] ExtendEncryptionKey = [0x73, 0x41, 0x6C, 0x54];

    public static Aes InitializeWithoutGenerateIV(ReadOnlySpan<byte> key, int object_number, int generation_number)
    {
        // Treating the object number and generation number as binary integers,
        // extend the original n-byte encryption key to n + 5 bytes by appending the low-order 3 bytes of the object number and the low-order 2 bytes of the generation number in that order, low-order byte first.
        // (n is 5 unless the value of V in the encryption dictionary is greater than 1, in which case n is the value of Length divided by 8.)
        var hash = MD5.HashData([
                .. key,
                (byte)object_number,
                (byte)(object_number >> 8),
                (byte)(object_number >> 16),
                (byte)generation_number,
                (byte)(generation_number >> 8),
                .. ExtendEncryptionKey,
            ]);

        // Use the first (n + 5) bytes, up to a maximum of 16, of the output from the MD5 hash as the key for the RC4 or AES symmetric key algorithms, along with the string or stream data to be encrypted.
        // If using the AES algorithm, the Cipher Block Chaining (CBC) mode, which requires an initialization vector, is used.
        // The block size parameter is set to 16 bytes, and the initialization vector is a 16-byte random number that is stored as the first 16 bytes of the encrypted stream or string.
        var aes = Aes.Create();
        aes.Key = hash[0..Math.Min(key.Length + 5, 16)];
        aes.Mode = CipherMode.CBC;
        aes.BlockSize = 16 * 8;
        return aes;
    }

    public (PipeWriter Input, PipeReader Output) CreateEncrypterPipe(int object_number, int generation_number)
    {
        var input = new Pipe();
        var output = new Pipe();

        _ = Task.Run(async () =>
        {
            using var aes = InitializeWithoutGenerateIV(Key, object_number, generation_number);
            aes.GenerateIV();

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
            using var aes = InitializeWithoutGenerateIV(Key, object_number, generation_number);
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
        var aes = InitializeWithoutGenerateIV(Key, object_number, generation_number);
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

    public IConverter CreateDecrypterConverter(int object_number, int generation_number)
    {
        var aes = InitializeWithoutGenerateIV(Key, object_number, generation_number);
        return new ConverterBinder()
        {
            Convert = bytes => aes.DecryptCbc(bytes[16..], bytes[0..16]),
            Dispose = () => aes.Dispose(),
        };
    }

    public static IEnumerable<byte> PadOrTruncatePassword32Bytes(ReadOnlySpan<byte> password) => ((byte[])[.. password, .. PasswordPaddingBytes]).Take(32);

    public static byte[] ComputeEncryptionKey_Algorithm2(
            ReadOnlySpan<byte> user_password,
            ReadOnlySpan<byte> owner_password,
            UserAccessPermissions permissions,
            byte[]? document_id,
            bool metadata_encrypted
        )
    {
        var p = (uint)(UserAccessPermissions.Default | permissions);
        var hash = MD5.HashData([
                .. PadOrTruncatePassword32Bytes(user_password),
                .. PadOrTruncatePassword32Bytes(owner_password),
                (byte)p,
                (byte)(p >> 8),
                (byte)(p >> 16),
                (byte)(p >> 24),
                .. document_id ?? [],
                .. metadata_encrypted ? (byte[])[] : [0xFF, 0xFF, 0xFF, 0xFF],
            ]);
        for (var i = 0; i < 50; i++) hash = MD5.HashData(hash);
        return hash;
    }

    public static byte[] ComputeOwnerPassword_Algorithm3(ReadOnlySpan<byte> user_password, ReadOnlySpan<byte> owner_password, int size)
    {
        var hash = MD5.HashData([.. PadOrTruncatePassword32Bytes(owner_password)]);
        for (var i = 0; i < 50; i++) hash = MD5.HashData(hash[0..size]);

        var owner_key = PadOrTruncatePassword32Bytes(user_password).ToArray();
        Span<byte> key = stackalloc byte[size];
        for (var i = 0; i < 20; i++)
        {
            for (var j = 0; j < size; j++) key[j] = (byte)(hash[j] ^ i);
            _ = Arcfour.Encrypt(Arcfour.InitializeKey(key), owner_key);
        }
        return owner_key;
    }

    public static byte[] ComputeUserPassword_Algorithm5(ReadOnlySpan<byte> document_id, ReadOnlySpan<byte> encryption_key)
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
