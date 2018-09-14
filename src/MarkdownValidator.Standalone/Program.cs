/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing.Parsers.CodeBlockParsers.Csharp.Telegram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MihaZupan.MarkdownValidator.Standalone
{
    class Program
    {
        static FileSystemWatcher FSWatcher;
        static MarkdownValidator Validator;

        static void Main(string[] args)
        {
            string location = args.Length == 0 ? "" : args[0];
            if (location == "")
                location = Environment.CurrentDirectory;

            location = "Wiki";
            location = @"C:\MihaZupan\MarkdownReferenceValidator\test-data\src";
            //location = "test";

            location = Path.GetFullPath(location);

            var config = new Config(location);
            config.Parsing.Warnings_HugeFile_LineCount = 600;
            config.Parsing.CodeBlocks.AddParser(new TelegramBotCodeBlockParser());

            Validator = new MarkdownValidator(config);
            FSWatcher = new FileSystemWatcher(location);

            FSWatcher.Renamed += (s, e) =>
            {
                if (IsMarkdownFile(e.FullPath))
                {
                    Validator.RemoveMarkdownFile(e.FullPath);
                    Validator.AddMarkdownFile(e.FullPath, GetSource(e.FullPath));
                }
                else
                {
                    Validator.RemoveEntity(e.FullPath);
                    Validator.AddEntity(e.FullPath);
                }

                Update();
            };
            FSWatcher.Deleted += (s, e) =>
            {
                if (IsMarkdownFile(e.FullPath))
                {
                    Validator.RemoveMarkdownFile(e.FullPath);
                }
                else
                {
                    Validator.RemoveEntity(e.FullPath);
                }

                Update();
            };
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
            FSWatcher.EnableRaisingEvents = true;

            Stack<string> directories = new Stack<string>();
            directories.Push(location);
            while (directories.Count > 0)
            {
                var directory = directories.Pop();
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
                    else
                    {
                        Validator.AddEntity(file);
                    }
                }
            }
            Update();

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
        static void UpdateCallback(object state)
        {
            lock (Validator)
            {
                var report = Validator.Validate();
                Console.Clear();

                if (report.Warnings.Count == 0)
                {
                    Console.WriteLine("All is good");
                    return;
                }

                int maxDocumentLength = report.Warnings.Max(w => w.Location.RelativeFilePath.Length);
                maxDocumentLength = Math.Max(8, maxDocumentLength);

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Severity\tDocument" + new string(' ', maxDocumentLength - 8) + "\tLine\tMessage");
                Console.ForegroundColor = color;

                foreach (var warning in report.Warnings)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}",
                        warning.IsError ? "Error\t" : (warning.IsSuggestion ? "Suggestion" : "Warning\t"),
                        warning.Location.RelativeFilePath.PadRight(maxDocumentLength, ' '),
                        warning.Location.RefersToEntireFile ? "n/a" : (warning.Location.Line + 1).ToString(),
                        warning.Message);
                }

                if (!report.IsComplete)
                    UpdateTimer.Change(report.SuggestedWait, Timeout.Infinite);
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
    }
}
