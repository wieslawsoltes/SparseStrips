// Copyright 2025 Wieslaw Soltes
// SPDX-License-Identifier: Apache-2.0 OR MIT

using BenchmarkDotNet.Running;
using Vello.Benchmarks;

// Run both Vello and SkiaSharp benchmarks
BenchmarkRunner.Run<ApiBenchmarks>();
BenchmarkRunner.Run<SkiaSharpBenchmarks>();
