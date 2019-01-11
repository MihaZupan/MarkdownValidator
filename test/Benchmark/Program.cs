using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Markdig;
using MihaZupan.MarkdownValidator;
using MihaZupan.MarkdownValidator.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<PerformanceVersusMarkdig>();
        }
    }

    [MemoryDiagnoser]
    public class PerformanceVersusMarkdig
    {
        private List<(string File, string Source)> Sources = new List<(string File, string Source)>();
        private List<string> Entities = new List<string>();
        private const string Root = @"C:\MihaZupan\Telegram\Telegram.Bot.Wiki\src";

        [Params(1, 10, 100)]
        public int N;

        [GlobalSetup]
        public void Setup()
        {
            Stack<string> directories = new Stack<string>();
            directories.Push(Root);
            while (directories.Count > 0)
            {
                string directory = directories.Pop();
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    directories.Push(dir);
                    Entities.Add(dir);
                }
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (file.EndsWith(".md", StringComparison.Ordinal))
                    {
                        Sources.Add((file, File.ReadAllText(file)));
                    }
                    else Entities.Add(file);
                }
            }
        }

        [Benchmark(Baseline = true)]
        public void Markdig()
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UsePreciseSourceLocation()
                .UseAutoLinks()
                .UseFootnotes()
                .Build();

            for (int i = 0; i < N; i++)
            {
                foreach (var (File, Source) in Sources)
                {
                    Markdown.Parse(Source, pipeline);
                }
            }
        }

        [Benchmark]
        public void MarkdownValidator()
        {
            var config = new Config(Root);
            config.WebIO.Enabled = false;
            MarkdownContextValidator validator = new MarkdownContextValidator(config);

            foreach (var entity in Entities) validator.AddEntity(entity);
            foreach (var (File, Source) in Sources)
            {
                validator.AddMarkdownFile(File, Source);
            }
            validator.Validate();
            for (int i = 1; i < N; i++)
            {
                foreach (var (File, Source) in Sources)
                {
                    validator.UpdateMarkdownFile(File, Source);
                }
                validator.Validate();
            }
        }
    }
}
