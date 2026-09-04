using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace PicoPDF.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        var config = new ManualConfig()
            .AddJob(Job.Default.WithArguments([new MsBuildArgument("/p:IsBenchmark=true")]))
            .AddLogger(NullLogger.Instance)
            .AddExporter(new MarkdownConsoleExporter())
            .AddColumnProvider(DefaultColumnProviders.Instance)
            .WithOption(ConfigOptions.DisableOptimizationsValidator, true)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
        BenchmarkRunner.Run(typeof(Program).Assembly, config, args);
#else
        var config = DefaultConfig.Instance
            .AddJob(Job.Default.WithArguments([new MsBuildArgument("/p:IsBenchmark=true")]))
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
#endif
    }
}
