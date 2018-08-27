using Newtonsoft.Json;
using System;
using System.IO;

namespace ReferenceValidator
{
    class Program
    {
        private static bool ErrorsFound = true;

        static void Main(string[] args)
        {
            if (!args.Length.IsInRange(0, 2))
            {
                PrintCriticalError($"Invalid argument list!");
                PrintUsage();
            }
            else
            {
                if (args[0].OrdinalContains("-h") || args[0].Contains('?'))
                {
                    PrintUsage();
                    ErrorsFound = true;
                }
                else
                {
                    string rootDirectory = args.Length == 1 ? args[0] : "src";
                    PrintDiagnosticLine($"Using directory: `{Path.GetFullPath(rootDirectory)}`");

                    if (!Directory.Exists(rootDirectory))
                    {
                        PrintCriticalError($"Could not find the root directory `{rootDirectory}`");
                    }
                    else
                    {
                        Report report = Validator.Validate(rootDirectory);
                        ErrorsFound = report.HasErrors;

                        PrintDiagnosticLine($"Indexed {report.AllFiles} files, {report.MarkdownFiles} of which are markdown files");

                        if (report.Warnings.Count != 0)
                        {
                            PrintDiagnosticLine($"\nFound {report.Warnings.Count} warning{(report.Warnings.Count % 100 == 1 ? "" : "s")}:");
                            foreach (var warning in report.Warnings)
                            {
                                PrintWarning(warning);
                            }
                        }
                        else
                        {
                            PrintColor("All references are good!\n", ConsoleColor.Green);
                        }

                        string reportPath = args.Length > 1
                            ? Path.GetFullPath(rootDirectory, args[1])
                            : Path.Combine(Environment.CurrentDirectory, "report.json");

                        if (args.Length < 2 || args[1] != "noreport")
                        {
                            PrintDiagnosticLine("Saving report to: " + Path.GetFullPath(reportPath));
                            string json = JsonConvert.SerializeObject(report, Formatting.Indented);
                            File.WriteAllText(reportPath, json);
                        }
                    }
                }
            }

            if (ErrorsFound)
                Environment.ExitCode = 1;
        }

        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:\tReferenceValidator [rootDirectory] [reportPath]");
            Console.WriteLine("\trootDirectory\t- the directory to look for markdown files, defaults to `WORKDIR\\src`");
            Console.WriteLine("\treportPath\t- the location for the report json, defaults to `WORKDIR\\report.json`");
            Console.WriteLine("Note:");
            Console.WriteLine("\tBoth arguments are optional");
            Console.WriteLine("\tYou can disable generating the report json file by passing `noreport` as `reportPath`");
            Console.WriteLine();
        }
        private static void PrintColor(string message, ConsoleColor color)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(message);
            Console.ForegroundColor = previousColor;
        }
        private static void PrintDiagnosticLine(string line)
            => PrintColor(line + '\n', ConsoleColor.Yellow);
        private static void PrintCriticalError(string message)
        {
            PrintColor("Critical error in the test framework: ", ConsoleColor.Red);
            Console.WriteLine(message);
            ErrorsFound = true;
        }
        private static void PrintWarning(Report.Warning warning)
        {
            if (warning.IsError)
                PrintColor($"Markdown error: ", ConsoleColor.Red);
            else
                PrintColor($"Markdown warning: ", ConsoleColor.Magenta);

            Console.WriteLine(warning.Message);
        }
    }
}
