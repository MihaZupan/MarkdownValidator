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

namespace MihaZupan.MarkdownValidator.Standalone
{
    class Program
    {
        static FileSystemWatcher FSWatcher;
        static MarkdownContextValidator Validator;

        static void Main(string[] args)
        {
            string location = args.Length == 0 ? "" : args[0];

            location = "Wiki";
            location = @"C:\MihaZupan\MarkdownValidator\test-data\src";
            //location = Path.Combine(Environment.CurrentDirectory, "../../../../../");
            location = "test";
            //location = @"C:\Users\Mihu\Downloads\docs-master\docs-master";
            location = @"C:\MihaZupan\Telegram\Telegram.Bot.Wiki";

            if (location == "")
                location = Environment.CurrentDirectory;
            else
                location = Path.GetFullPath(location);

            if (Environment.UserInteractive)
                Console.Title = "Markdown Validator - " + location;

            var config = new Config(location);
            Validator = new MarkdownContextValidator(config);
            File.WriteAllText(".markdown-validator", JsonConvert.SerializeObject(config, Formatting.Indented));

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
                        Validator.AddMarkdownFile(file, GetSource(file));
                    }
                    else Validator.AddEntity(file);
                }
            }

            Update();

            FSWatcher.EnableRaisingEvents = true;

            while (true) Console.ReadKey(true);
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

                if (!report.IsComplete)
                {
                    UpdateTimer.Change(500, Timeout.Infinite);
                }

                if (report != PreviousReport)
                {
                    PreviousReport = report;

                    Console.Clear();

                    if (report.WarningsByFile.Count == 0)
                    {
                        if (report.IsComplete)
                            WriteLineWithColor("All is good", ConsoleColor.Green);
                    }
                    else
                    {
                        int maxMessageLength = 150;
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
                    }

                    if (!report.IsComplete)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Some changes are still pending ...");
                    }
                }
            }
        }

        static bool IsMarkdownFile(string path)
        {
            return path.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        }
        static string GetSource(string path)
        {
            try
            {
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return new StreamReader(fs).ReadToEndAsync().Result;
                }
            }
            catch // Just try again once
            {
                using (var fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    return new StreamReader(fs).ReadToEndAsync().Result;
                }
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
