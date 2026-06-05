using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PicoPDF.Pdf.Documents.Security;

public class Aes128Handler : ISecurityHandler
{
    public required byte[] Key { get; init; }

    // If using the AES algorithm, extend the encryption key an additional 4 bytes by adding the value "sAlT", which corresponds to the hexadecimal values 0x73, 0x41, 0x6C, 0x54.
    // (This addition is done for backward compatibility and is not intended to provide additional security.)
    public static readonly byte[] ExtendEncryptionKey = [0x73, 0x41, 0x6C, 0x54];

    public static Aes InitializeWithoutGenerateIV(byte[] key, int object_number, int generation_number)
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
}
