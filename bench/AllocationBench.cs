using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Pdf.Documents.Security;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PicoPDF.Benchmark;

public class AllocationBench
{
    public static readonly Consumer Consumer = new();

    [Benchmark]
    public void Heap_Pad()
    {
        var user_password = Encoding.UTF8.GetBytes("xyz987");
        for (var loop = 0; loop < 1_000; loop++)
        {
            // Consumer.Consume cannot be called in the stack version.
            // The heap version also discards the return value.
            var bytes = new byte[32];
            Aes128Handler.PadOrTruncatePassword32Bytes(user_password, bytes);
            // Consumer.Consume(result);
        }
    }

    [Benchmark]
    public void Heap_Algorithm3()
    {
        var user_password = Encoding.UTF8.GetBytes("xyz987");
        Span<byte> owner_password = stackalloc byte[32];
        Aes128Handler.ComputeOwnerPassword_Algorithm3(
            user_password,
            Encoding.UTF8.GetBytes("abc123"),
            16,
            owner_password
        );
        for (var loop = 0; loop < 1_000; loop++)
        {
            var result = Aes128Handler_ComputeEncryptionKey_Algorithm2_Heap(
                user_password,
                owner_password,
                UserAccessPermissions.AllowPrint | UserAccessPermissions.AllowPrintFaithfulDigitalCopy,
                [
                    0xb8, 0xd6, 0xf3, 0x60, 0x1e, 0x6c, 0xa4, 0x99, 0x02, 0xf8, 0x93, 0xa9, 0x86, 0xcc, 0x71, 0x9f,
                    0x50, 0x76, 0x9d, 0x17, 0xed, 0xd1, 0x56, 0x27, 0xf3, 0x92, 0x19, 0xeb, 0x6d, 0xcc, 0xbe, 0x37,
                    0x02, 0xa7, 0x77, 0x43, 0xd9, 0x0f, 0xf4, 0x45, 0x9a, 0x86, 0xf6, 0x67, 0xab, 0x3f, 0x1e, 0xe4,
                    0xb4, 0x08, 0x07, 0x61, 0x22, 0xb4, 0x71, 0xe5, 0xad, 0x27, 0xa7, 0x09, 0x8f, 0xc1, 0x53, 0x05,
                ],
                true);
            Consumer.Consume(result);
        }
    }

    public byte[] Aes128Handler_ComputeEncryptionKey_Algorithm2_Heap(
            ReadOnlySpan<byte> user_password,
            ReadOnlySpan<byte> owner_password,
            UserAccessPermissions permissions,
            byte[]? document_id,
            bool metadata_encrypted
        )
    {
        var p = (uint)(UserAccessPermissions.Default | permissions);
        var u = new byte[32];
        var o = new byte[32];
        Aes128Handler.PadOrTruncatePassword32Bytes(user_password, u);
        Aes128Handler.PadOrTruncatePassword32Bytes(owner_password, o);
        var hash = MD5.HashData([
                .. u,
                .. o,
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

    [Benchmark]
    public void Stack_Pad()
    {
        var user_password = Encoding.UTF8.GetBytes("xyz987");
        Span<byte> result = stackalloc byte[32];
        for (var loop = 0; loop < 1_000; loop++)
        {
            Aes128Handler.PadOrTruncatePassword32Bytes(user_password, result);
        }
    }

    [Benchmark]
    public void Stack_Algorithm3()
    {
        var user_password = Encoding.UTF8.GetBytes("xyz987");
        Span<byte> owner_password = stackalloc byte[32];
        Aes128Handler.ComputeOwnerPassword_Algorithm3(
            user_password,
            Encoding.UTF8.GetBytes("abc123"),
            16,
            owner_password
        );
        for (var loop = 0; loop < 1_000; loop++)
        {
            var result = Aes128Handler_ComputeEncryptionKey_Algorithm2_Stack(
                user_password,
                owner_password,
                UserAccessPermissions.AllowPrint | UserAccessPermissions.AllowPrintFaithfulDigitalCopy,
                [
                    0xb8, 0xd6, 0xf3, 0x60, 0x1e, 0x6c, 0xa4, 0x99, 0x02, 0xf8, 0x93, 0xa9, 0x86, 0xcc, 0x71, 0x9f,
                    0x50, 0x76, 0x9d, 0x17, 0xed, 0xd1, 0x56, 0x27, 0xf3, 0x92, 0x19, 0xeb, 0x6d, 0xcc, 0xbe, 0x37,
                    0x02, 0xa7, 0x77, 0x43, 0xd9, 0x0f, 0xf4, 0x45, 0x9a, 0x86, 0xf6, 0x67, 0xab, 0x3f, 0x1e, 0xe4,
                    0xb4, 0x08, 0x07, 0x61, 0x22, 0xb4, 0x71, 0xe5, 0xad, 0x27, 0xa7, 0x09, 0x8f, 0xc1, 0x53, 0x05,
                ],
                true);
            Consumer.Consume(result);
        }
    }

    public byte[] Aes128Handler_ComputeEncryptionKey_Algorithm2_Stack(
            ReadOnlySpan<byte> user_password,
            ReadOnlySpan<byte> owner_password,
            UserAccessPermissions permissions,
            byte[]? document_id,
            bool metadata_encrypted
        )
    {
        Span<byte> user_span = stackalloc byte[32];
        Span<byte> owner_span = stackalloc byte[32];
        Aes128Handler.PadOrTruncatePassword32Bytes(user_password, user_span);
        Aes128Handler.PadOrTruncatePassword32Bytes(owner_password, owner_span);

        var p = (uint)(UserAccessPermissions.Default | permissions);
        var hash = MD5.HashData([
                .. user_span,
                .. owner_span,
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
}
