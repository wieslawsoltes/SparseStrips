// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Vello.Benchmarks;

// Use BenchmarkSwitcher to allow running benchmarks in groups:
// - To run API benchmarks comparison:      dotnet run -c Release -- --filter *Api*
// - To run Overhead benchmarks comparison: dotnet run -c Release -- --filter *Overhead*
// - To run all benchmarks:                 dotnet run -c Release

var switcher = BenchmarkSwitcher.FromTypes(new[]
{
    typeof(VelloApiBenchmarks),
    typeof(SkiaSharpApiBenchmarks),
    typeof(VelloOverheadBenchmarks),
    typeof(SkiaSharpOverheadBenchmarks)
});

switcher.Run(args, ManualConfig.Create(DefaultConfig.Instance)
    .WithSummaryStyle(BenchmarkDotNet.Reports.SummaryStyle.Default
        .WithMaxParameterColumnWidth(40)));
