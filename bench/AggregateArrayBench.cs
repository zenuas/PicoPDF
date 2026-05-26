using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.Collections.Generic;
using System.Linq;

namespace PicoPDF.Benchmark;

public class AggregateArrayBench
{
    public struct SectionData
    {
        public int BreakCount;
        public string Section;
        public int Depth;
        public string BreakKey;
    }

    public static readonly Consumer Consumer = new();
    public static readonly SectionData[] Sections1 =
        [
            new (){ BreakCount = 0, Section = "Section1", Depth = 1, BreakKey = "BreakKey1" },
        ];
    public static readonly SectionData[] Sections5 =
        [
            new (){ BreakCount = 0, Section = "Section1", Depth = 1, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section2", Depth = 2, BreakKey = "BreakKey2" },
            new (){ BreakCount = 0, Section = "Section3", Depth = 3, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section4", Depth = 4, BreakKey = "BreakKey4" },
            new (){ BreakCount = 0, Section = "Section5", Depth = 5, BreakKey = "BreakKey5" },
        ];
    public static readonly SectionData[] Sections10 =
        [
            new (){ BreakCount = 0, Section = "Section1", Depth = 1, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section2", Depth = 2, BreakKey = "BreakKey2" },
            new (){ BreakCount = 0, Section = "Section3", Depth = 3, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section4", Depth = 4, BreakKey = "BreakKey4" },
            new (){ BreakCount = 0, Section = "Section5", Depth = 5, BreakKey = "BreakKey5" },
            new (){ BreakCount = 0, Section = "Section6", Depth = 6, BreakKey = "BreakKey6" },
            new (){ BreakCount = 0, Section = "Section7", Depth = 7, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section8", Depth = 8, BreakKey = "" },
            new (){ BreakCount = 0, Section = "Section9", Depth = 9, BreakKey = "BreakKey9" },
            new (){ BreakCount = 0, Section = "Section10", Depth = 10, BreakKey = "BreakKey10" },
        ];

    [Benchmark]
    public void Array1()
    {
        var break_count = 0;
        var hierarchy = new (int BreakCount, string Section, int Depth, string BreakKey)[Sections1.Length];
        for (var i = 0; i < Sections1.Length; i++)
        {
            var x = Sections1[i];
            hierarchy[i] = (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey);
        }

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void Array5()
    {
        var break_count = 0;
        var hierarchy = new (int BreakCount, string Section, int Depth, string BreakKey)[Sections5.Length];
        for (var i = 0; i < Sections5.Length; i++)
        {
            var x = Sections5[i];
            hierarchy[i] = (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey);
        }

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void Array10()
    {
        var break_count = 0;
        var hierarchy = new (int BreakCount, string Section, int Depth, string BreakKey)[Sections10.Length];
        for (var i = 0; i < Sections10.Length; i++)
        {
            var x = Sections10[i];
            hierarchy[i] = (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey);
        }

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToArray1()
    {
        var break_count = 0;
        var hierarchy = Sections1
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToArray();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToArray5()
    {
        var break_count = 0;
        var hierarchy = Sections5
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToArray();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToArray10()
    {
        var break_count = 0;
        var hierarchy = Sections10
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToArray();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToList1()
    {
        var break_count = 0;
        var hierarchy = Sections1
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToList();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToList5()
    {
        var break_count = 0;
        var hierarchy = Sections5
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToList();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void SelectToList10()
    {
        var break_count = 0;
        var hierarchy = Sections10
            .Select(x => (BreakCount: x.BreakKey != "" ? ++break_count : break_count, x.Section, x.Depth, x.BreakKey))
            .ToList();

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateList1()
    {
        var hierarchy = Sections1
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections1.Length),
                (acc, x) =>
                {
                    acc.Add((BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey));
                    return acc;
                });

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateList5()
    {
        var hierarchy = Sections5
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections5.Length),
                (acc, x) =>
                {
                    acc.Add((BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey));
                    return acc;
                });

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateList10()
    {
        var hierarchy = Sections10
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections10.Length),
                (acc, x) =>
                {
                    acc.Add((BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey));
                    return acc;
                });

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateListEx1()
    {
        var hierarchy = Sections1
            .Aggregate(
                (BreakCount: 0, List: new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections1.Length)),
                (acc, x) =>
                {
                    acc.List.Add((BreakCount: x.BreakKey != "" ? ++acc.BreakCount : acc.BreakCount, x.Section, x.Depth, x.BreakKey));
                    return acc;
                }).List;

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateListEx5()
    {
        var hierarchy = Sections5
            .Aggregate(
                (BreakCount: 0, List: new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections5.Length)),
                (acc, x) =>
                {
                    acc.List.Add((BreakCount: x.BreakKey != "" ? ++acc.BreakCount : acc.BreakCount, x.Section, x.Depth, x.BreakKey));
                    return acc;
                }).List;

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void AggregateListEx10()
    {
        var hierarchy = Sections10
            .Aggregate(
                (BreakCount: 0, List: new List<(int BreakCount, string Section, int Depth, string BreakKey)>(Sections10.Length)),
                (acc, x) =>
                {
                    acc.List.Add((BreakCount: x.BreakKey != "" ? ++acc.BreakCount : acc.BreakCount, x.Section, x.Depth, x.BreakKey));
                    return acc;
                }).List;

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void Aggregate1()
    {
        var hierarchy = Sections1
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(),
                (acc, x) => [.. acc, (BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey)]);

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void Aggregate5()
    {
        var hierarchy = Sections5
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(),
                (acc, x) => [.. acc, (BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey)]);

        Consumer.Consume(hierarchy);
    }

    [Benchmark]
    public void Aggregate10()
    {
        var hierarchy = Sections10
            .Aggregate(
                new List<(int BreakCount, string Section, int Depth, string BreakKey)>(),
                (acc, x) => [.. acc, (BreakCount: acc.LastOrDefault().BreakCount + (x.BreakKey != "" ? 1 : 0), x.Section, x.Depth, x.BreakKey)]);

        Consumer.Consume(hierarchy);
    }
}
