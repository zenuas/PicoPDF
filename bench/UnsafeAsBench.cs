using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System;
using System.Runtime.CompilerServices;

namespace PicoPDF.Benchmark;

public class UnsafeAsBench
{
    public enum TestEnum
    {
        Value1 = 1,
        Value2 = 2,
        Value3 = 3,
    }

    public Consumer Consumer { get; init; } = new();

    [Benchmark]
    public void UnsafeAs_ByteInt()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            byte b = 10;
            Consumer.Consume(Unsafe.As<byte, int>(ref b));
        }
    }

    [Benchmark]
    public void UnsafeAs_ByteEnum()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            byte b = 10;
            Consumer.Consume(Unsafe.As<byte, TestEnum>(ref b));
        }
    }

    [Benchmark]
    public void UnsafeAs_ByteT()
    {
        byte b = 10;
        Convert_TEnum2<TestEnum>(b);
    }

    [Benchmark]
    public void Default_ByteInt()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            byte b = 10;
            Consumer.Consume((int)b);
        }
    }

    [Benchmark]
    public void Default_ByteEnum()
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            byte b = 10;
            Consumer.Consume((TestEnum)b);
        }
    }

    [Benchmark]
    public void Default_ByteT()
    {
        byte b = 10;
        Convert_TEnum<TestEnum>(b);
    }

    public void Convert_TEnum<T>(byte a) where T : struct, Enum
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            Consumer.Consume((T)(object)(int)a);
        }
    }

    public void Convert_TEnum2<T>(byte a) where T : struct, Enum
    {
        for (var i = 0; i < 1_000_000; i++)
        {
            Consumer.Consume(Unsafe.As<byte, T>(ref a));
        }
    }
}
