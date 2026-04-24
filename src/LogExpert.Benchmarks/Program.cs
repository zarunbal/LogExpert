using BenchmarkDotNet.Running;

namespace LogExpert.Benchmarks;

public static class Program
{
    public static void Main (string[] args)
    {
        //_ = BenchmarkRunner.Run<StreamReaderBenchmarks>();
        _ = BenchmarkRunner.Run<BufferIndexBenchmarks>();
    }
}

/*
 * Comment / Uncommen the benchmark to run, careful some can run longer
 * 1.) a dry run
 * dotnet run -c Release --job Dry --noOverwrite
 * 2.) a short run
 * dotnet run -c Release --job Short --noOverwrite
 * 3.) a full baseline run
 * dotnet run -c Release --noOverwrite
 *
 * The full baseline run generates a MD file
 * BenchmarkDotNet.Artifacts/results/*-report-github.md
 *
 * If changes are made with the LogfileReader / BufferIndex, always do a Benchmark to
 * verify no performance regression is introduced, especially with large files.
 */
