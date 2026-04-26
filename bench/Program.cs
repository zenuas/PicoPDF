using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
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
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
        config.AddLogger(NullLogger.Instance);
        config.AddExporter(new MarkdownConsoleExporter());
        config.AddColumnProvider(DefaultColumnProviders.Instance);
        config.WithOption(ConfigOptions.DisableOptimizationsValidator, true);
        BenchmarkRunner.Run(typeof(Program).Assembly, config, args);
#else
        var config = DefaultConfig.Instance
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.Declared));
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
#endif
    }
}
