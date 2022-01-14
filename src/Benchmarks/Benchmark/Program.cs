﻿/*
 * Copyright 2018 James Courtney
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Benchmark
{
    using System;
    using System.Collections.Generic;

    using BenchmarkDotNet.Columns;
    using BenchmarkDotNet.Configs;
    using BenchmarkDotNet.Diagnosers;
    using BenchmarkDotNet.Environments;
    using BenchmarkDotNet.Exporters;
    using BenchmarkDotNet.Jobs;
    using BenchmarkDotNet.Loggers;
    using BenchmarkDotNet.Reports;
    using BenchmarkDotNet.Running;

    public class Program
    {
        public static void Main(string[] args)
        {
            List<Summary> summaries = new List<Summary>();

            Job job = Job.ShortRun
                .WithAnalyzeLaunchVariance(true)
                .WithLaunchCount(7)
                .WithWarmupCount(3)
                .WithIterationCount(5)
                .WithRuntime(CoreRuntime.Core60);
                //.WithEnvironmentVariable(new EnvironmentVariable("DOTNET_TieredPGO", "1"));

            var config = DefaultConfig.Instance
                 .AddColumn(new[] { StatisticColumn.P25, StatisticColumn.P95 })
                 .AddDiagnoser(MemoryDiagnoser.Default)
                 .AddJob(job);

            summaries.Add(BenchmarkRunner.Run(typeof(FBBench.FBSerializeBench), config));
            summaries.Add(BenchmarkRunner.Run(typeof(FBBench.FBDeserializeBench), config));
#if RUN_COMPARISON_BENCHMARKS
            summaries.Add(BenchmarkRunner.Run(typeof(FBBench.OthersDeserializeBench), config));
#endif

#if FLATSHARP_6_0_0_OR_GREATER
            summaries.Add(BenchmarkRunner.Run(typeof(FBBench.FBSharedStringBench), config));
            summaries.Add(BenchmarkRunner.Run(typeof(FBBench.WriteThroughBench), config));
#endif
#if CURRENT_VERSION_ONLY
            summaries.Add(BenchmarkRunner.Run(typeof(SerializationContextBenchmark), config));
#endif

            foreach (var item in summaries)
            {
                MarkdownExporter.Console.ExportToLog(item, new ConsoleLogger());
                MarkdownExporter.GitHub.ExportToFiles(item, new ConsoleLogger());
            }
        }
    }
}
