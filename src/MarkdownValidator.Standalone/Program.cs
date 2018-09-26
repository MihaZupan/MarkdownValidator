/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MihaZupan.MarkdownValidator.Standalone
{
    class Program
    {
        static FileSystemWatcher FSWatcher;
        static MarkdownContextValidator Validator;

        public const string ConfigurationFileName = ".markdown-validator.json";
        public const string DefaultConfigurationFileName = ".markdown-validator-default.json";

        static void Main(string[] args)
        {
            File.WriteAllText(DefaultConfigurationFileName, JsonConvert.SerializeObject(new Config(string.Empty), Formatting.Indented));
            string location = args.Length == 0 ? "" : args[0];

            location = "Wiki";
            //location = @"C:\MihaZupan\MarkdownReferenceValidator\test-data\src";
            //location = Path.Combine(Environment.CurrentDirectory, "../../../../../");
            //location = "test";
            //location = @"C:\Users\Mihu\Downloads\docs-master\docs-master";

            if (location == "")
                location = Environment.CurrentDirectory;
            else
                location = Path.GetFullPath(location);

            if (Environment.UserInteractive)
                Console.Title = "Markdown Validator - " + location;

            Config configuration = null;
            string path = ConfigurationFileName;
            if (File.Exists(path))
            {
                configuration = TryLoadConfiguration(path);
            }
            if (configuration != null)
            {
                path = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), ConfigurationFileName);
                if (File.Exists(path))
                {
                    configuration = TryLoadConfiguration(path);
                }
            }
            if (configuration != null)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
                if (File.Exists(path))
                {
                    configuration = TryLoadConfiguration(path);
                }
            }

            if (configuration == null) configuration = new Config(location);
            else configuration.RootWorkingDirectory = location;

            Validator = new MarkdownContextValidator(configuration);
            FSWatcher = new FileSystemWatcher(location);
            FSWatcher.IncludeSubdirectories = true;

            FSWatcher.Renamed += (s, e) =>
            {
                Validator.RemoveEntity(e.FullPath);
                if (IsMarkdownFile(e.FullPath))
                {
                    Validator.AddMarkdownFile(e.FullPath, GetSource(e.FullPath));
                }
                else
                {
                    Validator.AddEntity(e.FullPath);
                }

                Update();
            };
            FSWatcher.Deleted += (s, e) => Validator.RemoveEntity(e.FullPath);
            FSWatcher.Created += (s, e) =>
            {
                if (IsMarkdownFile(e.FullPath))
                {
                    Validator.AddMarkdownFile(e.FullPath, GetSource(e.FullPath));
                }
                else
                {
                    Validator.AddEntity(e.FullPath);
                }

                Update();
            };
            FSWatcher.Changed += (s, e) =>
            {
                if (IsMarkdownFile(e.FullPath))
                {
                    Validator.UpdateMarkdownFile(e.FullPath, GetSource(e.FullPath));
                    Update();
                }
            };

            WriteLineWithColor("Indexing files ...", ConsoleColor.Green);

            List<string> files = new List<string>();
            Stack<string> directories = new Stack<string>();
            directories.Push(location);
            while (directories.Count > 0)
            {
                string directory = directories.Pop();
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    directories.Push(dir);
                    Validator.AddEntity(dir);
                }
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (IsMarkdownFile(file))
                    {
                        files.Add(file);
                    }
                    else Validator.AddEntity(file);
                }
            }

            List<(string File, string Source)> sources = new List<(string, string)>();
            int totalParsed = 0;
            var task = Task.Run(() =>
            {
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                    file =>
                {
                    string source = GetSource(file);
                    lock (sources)
                    {
                        sources.Add((file, source));
                    }
                });
                Parallel.ForEach(sources, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 },
                    fileSource =>
                {
                    Validator.AddMarkdownFile(fileSource.File, fileSource.Source);
                    Interlocked.Increment(ref totalParsed);
                });
            });

            int lastDone = -1;
            while (!task.IsCompleted)
            {
                int total = totalParsed;
                if (lastDone != total)
                {
                    lastDone = total;
                    string status;
                    if (total == 0)
                    {
                        status = "Reading files from disk";
                    }
                    else
                    {
                        status = string.Format("Parsed {0} out of {1} files. {2}% done",
                            total, files.Count, (int)((float)total / files.Count * 100));
                    }
                    Console.Clear();
                    WriteLineWithColor(status, ConsoleColor.Green);
                }
                Thread.Sleep(100);
            }
            Update();

            FSWatcher.EnableRaisingEvents = true;

            while (true) Console.ReadKey(true);
        }
        private static Config TryLoadConfiguration(string path)
        {
            string json = File.ReadAllText(path);
            try
            {
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch
            {
                return null;
            }
        }

        private static Timer UpdateTimer = new Timer(UpdateCallback);
        static void Update()
        {
            lock (Validator)
            {
                UpdateTimer.Change(50, Timeout.Infinite);
            }
        }

        static ValidationReport PreviousReport = null;
        static void UpdateCallback(object state)
        {
            lock (Validator)
            {
                var report = Validator.Validate();
                if (report == PreviousReport)
                    return;
                PreviousReport = report;

                Console.Clear();

                if (report.WarningsByFile.Count == 0)
                {
                    WriteLineWithColor("All is good", ConsoleColor.Green);
                    return;
                }

                int maxMessageLength = 100;
                int maxNameLength = Math.Max(8, report.WarningsByFile.Max(w => w.Key.Length));
                maxNameLength = Math.Min(maxNameLength, 45);

                WriteLineWithColor("Severity\tDocument" + new string(' ', maxNameLength - 8) + "\tLine\tMessage", ConsoleColor.Green);

                StringBuilder reportBuilder = new StringBuilder();
                int totalWarnings = report.WarningCount;
                int count = 0;
                foreach (var fileWarnings in report.WarningsByFile)
                {
                    if (count++ == 50)
                        break;
                    count += fileWarnings.Value.Count - 1;

                    string fileName = fileWarnings.Value.First().Location.RelativeFilePath;
                    if (fileName.Length > maxNameLength)
                    {
                        fileName = fileName.Substring(0, maxNameLength - 3) + "...";
                    }
                    else fileName = fileName.PadRight(maxNameLength, ' ');

                    foreach (var warning in fileWarnings.Value)
                    {

                        string message = warning.Message;
                        if (message.Length > maxMessageLength)
                            message = message.Substring(0, maxMessageLength - 3) + "...";

                        reportBuilder.AppendFormat("{0}\t{1}\t{2}\t{3}\n",
                            warning.IsError ? "Error\t" : (warning.IsSuggestion ? "Suggestion" : "Warning\t"),
                            fileName,
                            warning.Location.RefersToEntireFile ? "n/a" : (warning.Location.StartLine + 1).ToString(),
                            message);
                    }
                }
                Console.Write(reportBuilder.ToString());

                if (count < totalWarnings)
                {
                    Console.WriteLine();
                    Console.WriteLine("Showing the first {0} out of {1} warnings", count, totalWarnings);
                }

                if (!report.IsComplete)
                {
                    UpdateTimer.Change(Math.Min(100, report.SuggestedWait), Timeout.Infinite);

                    Console.WriteLine();
                    Console.WriteLine("Some changes are still pending ...");
                }
            }
        }

        static bool IsMarkdownFile(string path)
        {
            return path.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        }
        static string GetSource(string path)
        {
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                return new StreamReader(fs).ReadToEndAsync().Result;
            }
        }

        static void WriteLineWithColor(string line, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(line);
            Console.ForegroundColor = previousColor;
        }
    }
}
