/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing.Parsers.CodeBlockParsers.Csharp.Telegram;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            location = Path.GetFullPath(location);

            var config = new Config(location);
            config.Parsing.Warnings_HugeFile_LineCount = 500;
            config.Parsing.CodeBlocks.AddParser(new TelegramBotCodeBlockParser());

            Validator = new MarkdownValidator(config);
            FSWatcher = new FileSystemWatcher(location);

            FSWatcher.Renamed += (s, e) =>
            {
                bool isMarkdownFile = Path.GetExtension(e.FullPath).Length > 0 &&
                    e.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdownFile)
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
                bool isMarkdownFile = Path.GetExtension(e.FullPath).Length > 0 &&
                    e.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdownFile)
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
                bool isMarkdownFile = Path.GetExtension(e.FullPath).Length > 0 &&
                    e.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdownFile)
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
                bool isMarkdownFile = Path.GetExtension(e.FullPath).Length > 0 &&
                    e.FullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

                if (isMarkdownFile)
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
                    if (file.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
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

        static void Update()
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
                int spaceToAdd = Math.Max(0, maxDocumentLength - 8);

                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Severity\tDocument" + new string(' ', spaceToAdd) + "\tLine\tMessage");
                Console.ForegroundColor = color;

                foreach (var warning in report.Warnings)
                {
                    Console.WriteLine("{0}\t{1}\t{2}\t{3}",
                        warning.IsError ? "Error\t" : "Warning\t",
                        warning.Location.RelativeFilePath.PadRight(maxDocumentLength, ' '),
                        warning.Location.Line + 1,
                        warning.Message);
                }
            }
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
