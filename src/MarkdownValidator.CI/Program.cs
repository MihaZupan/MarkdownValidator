/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using McMaster.Extensions.CommandLineUtils;
using MihaZupan.MarkdownValidator.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace MihaZupan.MarkdownValidator.CI
{
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandLineApplication(false);

            app.HelpOption();

            var pathOption = app.Option("-p|--path", "Path to the root working directory if --config is not set (default: current directory)", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingDirectory());

            var configOption = app.Option("-c|--config", "Path to the configuration file", CommandOptionType.SingleValue)
                .Accepts(v => v.ExistingFile());

            var errorThresholdOption = app.Option<int>("--threshold", "Number of errors to allow before returning status code 1 (default: 0)", CommandOptionType.SingleValue);

            var warningIsErrorOption = app.Option<bool>("--warningIsError", "Treat warnings as errors (default: false)", CommandOptionType.SingleValue);

            var configOutputOption = app.Option("-co|--configOutput", "Path for the updated configuration file", CommandOptionType.SingleValue)
                .Accepts(v => v.LegalFilePath());

            app.OnExecute(() =>
            {
                int errorThreshold = errorThresholdOption.HasValue() ? errorThresholdOption.ParsedValue : 0;
                bool warningIsError = warningIsErrorOption.HasValue() ? warningIsErrorOption.ParsedValue : false;
                string configOutput = configOutputOption.HasValue() ? configOutputOption.Value() : null;

                Config config;
                if (configOption.HasValue())
                {
                    string configPath = configOption.Value();
                    try
                    {
                        string json = ReadTextFile(configPath);
                        config = JsonConvert.DeserializeObject<Config>(json);
                    }
                    catch
                    {
                        WriteLine("Failed to parse the configuration file at " + configPath, ConsoleColor.Red);
                        return 1;
                    }
                }
                else if (pathOption.HasValue())
                {
                    config = new Config(pathOption.Value());
                }
                else
                {
                    config = new Config(Environment.CurrentDirectory);
                }

                int exitCode = ProcessContext(config, errorThreshold, warningIsError);
                if (configOutput != null)
                {
                    File.WriteAllText(configOutput, JsonConvert.SerializeObject(config, Formatting.Indented));
                }
                return exitCode;
            });

            return app.Execute(args);
        }
        static int ProcessContext(Config config, int errorThreshold, bool warningIsError)
        {
            var validator = new MarkdownContextValidator(config);

            int markdownFiles = 0;

            Stack<string> directories = new Stack<string>();
            directories.Push(config.RootWorkingDirectory);
            while (directories.Count > 0)
            {
                string directory = directories.Pop();
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    directories.Push(dir);
                    validator.AddEntity(dir);
                }
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (IsMarkdownFile(file))
                    {
                        markdownFiles++;
                        validator.AddMarkdownFile(file, ReadTextFile(file));
                    }
                    else validator.AddEntity(file);
                }
            }

            Console.WriteLine("Validating {0} markdown files", markdownFiles);

            return Validate(validator, errorThreshold, warningIsError);
        }
        static int Validate(MarkdownContextValidator validator, int errorThreshold, bool warningIsError)
        {
            CancellationTokenSource tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1));
            var report = validator.ValidateFully(tokenSource.Token);

            if (tokenSource.IsCancellationRequested)
            {
                WriteLine("Failed to validate within a minute. This indicates a problem with a parser.", ConsoleColor.Red);
                return 1;
            }

            WriteLine("Validation complete.", ConsoleColor.Green);

            if (report.WarningCount == 0)
            {
                WriteLine("No problems found.", ConsoleColor.Green);
                return 0;
            }

            Console.WriteLine();
            PrintWarnings(report);

            int errorCount = warningIsError ? report.WarningCount : report.ErrorCount;
            return errorCount <= errorThreshold ? 0 : 1;
        }
        static void PrintWarnings(ValidationReport report)
        {
            WriteLine("Found " + report.WarningCount + " warnings", ConsoleColor.Red);

            foreach (var fileWarnings in report.WarningsByFile.OrderBy(w => w.Key, StringComparer.OrdinalIgnoreCase))
            {
                string fileName = fileWarnings.Key;

                Console.WriteLine();
                WriteLine(fileName + ":", ConsoleColor.DarkRed);

                int longestLinePrefix = 0;
                foreach (var warning in fileWarnings.Value)
                {
                    if (!warning.Location.RefersToEntireFile)
                    {
                        if (warning.Location.IsSingleLine)
                        {
                            longestLinePrefix = Math.Max(longestLinePrefix,
                                6 + (int)Math.Ceiling(Math.Log10(warning.Location.StartLine + 1)));
                        }
                        else
                        {
                            longestLinePrefix = Math.Max(longestLinePrefix,
                                8 + (int)Math.Ceiling(Math.Log10(warning.Location.StartLine + 1) +
                                    (int)Math.Ceiling(Math.Log10(warning.Location.EndLine + 1))));
                        }
                    }
                }
                longestLinePrefix++;

                foreach (var warning in fileWarnings.Value)
                {
                    Console.Write('\t');

                    if (!warning.Location.RefersToEntireFile)
                    {
                        string prefix = string.Empty;
                        if (warning.Location.IsSingleLine)
                        {
                            prefix = string.Concat("Line ", warning.Location.StartLine + 1, ":");
                        }
                        else
                        {
                            prefix = string.Concat("Lines ", warning.Location.StartLine + 1, "-", warning.Location.EndLine + 1, ":");
                        }
                        Write(prefix.PadRight(longestLinePrefix), ConsoleColor.Yellow);
                    }

                    Console.WriteLine(warning.Message);
                }
            }
            Console.WriteLine();
        }

        static void Write(string value, ConsoleColor color = ConsoleColor.White)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = previousColor;
        }
        static void WriteLine(string value, ConsoleColor color = ConsoleColor.White)
            => Write(value + Environment.NewLine, color);
        static readonly string[] MarkdownExtensions =
        {
            "md",
            "markdown",
            "mdown",
            "mkdn",
            "mkd"
        };
        static bool IsMarkdownFile(string path)
        {
            int indexOfLastDot = path.LastIndexOf('.');
            if (indexOfLastDot == -1) return false;
            indexOfLastDot++;

            foreach (var extension in MarkdownExtensions)
            {
                if (path.IndexOf(extension, indexOfLastDot, StringComparison.OrdinalIgnoreCase) != -1)
                    return true;
            }
            return false;
        }
        static string ReadTextFile(string path)
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
    }
}
