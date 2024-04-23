using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using BenchmarkDotNetVisualizer;
using NodaTime;
using SolCalc;
using SolCalc.Data;
using System.Diagnostics;

namespace Performance;

internal static class PerformanceTest {

    public static void Main(string[] args) {
        Summary summary = BenchmarkRunner.Run<SolCalcBenchmarks>(args: args);

        string reportPath = Path.Combine(summary.ResultsDirectoryPath, summary.BenchmarksCases.First().Descriptor.Type.FullName + "-report-rich.html");
        Console.WriteLine($"\nHTML report: {reportPath}");
        if (File.Exists(reportPath)) {
            Process.Start(new ProcessStartInfo(reportPath) { UseShellExecute = true })?.Dispose();
        }
    }

}

[RichHtmlExporter(
    title: nameof(SolCalcBenchmarks),
    spectrumColumns: ["Mean"],
    groupByColumns: ["Categories"],
    highlightGroups: false)]
[CategoriesColumn]
// [ShortRunJob]
public class SolCalcBenchmarks {

    private const double Latitude  = 37.35;
    private const double Longitude = -121.95;

    private DateTimeZone  _americaLosAngeles = null!;
    private ZonedDateTime _dateTime;

    [GlobalSetup]
    public void Setup() {
        _americaLosAngeles = DateTimeZoneProviders.Tzdb["America/Los_Angeles"];
        _dateTime          = new LocalDateTime(2024, 1, 28, 0, 0).InZoneStrictly(_americaLosAngeles);
    }

    [GlobalSetup(Target = nameof(GetAllSunlightChangesInDayByNewtonsRootFinderWithOneHundredthDegreePrecision))]
    public void SetPrecisionOneHundredthDegree() {
        SunlightCalculator.ElevationPrecision = 0.01m;
    }

    [GlobalSetup(Target = nameof(GetAllSunlightChangesInDayByNewtonsRootFinderWithTenThousandthDegreePrecision))]
    public void SetPrecisionTenThousandthDegree() {
        SunlightCalculator.ElevationPrecision = 0.0001m;
    }

    /*[Benchmark]
    [BenchmarkCategory("Get next sunlight change")]
    public SunlightChange GetNextSunlightChangeByNewtonsRootFinder() {
        return SunlightCalculator.GetSunlightChanges(_dateTime, Latitude, Longitude).First();
    }*/

    [Benchmark]
    [BenchmarkCategory("Get all sunlight changes in day")]
    public List<SunlightChange> GetAllSunlightChangesInDayByNewtonsRootFinderWithOneHundredthDegreePrecision() {
        return SunlightCalculator.GetSunlightChanges(_dateTime, Latitude, Longitude).TakeWhile(change => change.Time.Date == _dateTime.Date).ToList();
    }

    [Benchmark]
    [BenchmarkCategory("Get all sunlight changes in day")]
    public List<SunlightChange> GetAllSunlightChangesInDayByNewtonsRootFinderWithTenThousandthDegreePrecision() {
        return SunlightCalculator.GetSunlightChanges(_dateTime, Latitude, Longitude).TakeWhile(change => change.Time.Date == _dateTime.Date).ToList();
    }

}