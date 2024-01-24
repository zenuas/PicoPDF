using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Reports;
using System;
using System.Collections.Generic;

namespace PicoPDF.Benchmark;

public class MarkdownConsoleExporter : IExporter, ILogger
{
    public string Name => nameof(MarkdownConsoleExporter);

    public string Id => "";

    public int Priority => throw new NotImplementedException();

    public IEnumerable<string> ExportToFiles(Summary summary, ILogger consoleLogger)
    {
        ExportToLog(summary, consoleLogger);
        yield break;
    }

    public void ExportToLog(Summary summary, ILogger logger)
    {
        var md = MarkdownExporter.GitHub;
        md.ExportToLog(summary, this);
    }

    public void Flush() { }

    public void Write(LogKind logKind, string text) => Console.Write(text);

    public void WriteLine() => Console.WriteLine();

    public void WriteLine(LogKind logKind, string text) => Console.WriteLine(text);
}
