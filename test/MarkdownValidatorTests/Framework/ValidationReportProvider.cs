/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using System.Collections.Generic;
using System.IO;

namespace MihaZupan.MarkdownValidator.Tests.Framework
{
    static class ValidationReportProvider
    {
        private static readonly string RootDirectory = Path.GetFullPath("TestDirectory");

        private static readonly MarkdownValidator Validator
            = new MarkdownValidator(new Config(RootDirectory));

        public static ValidationReport GetReport(params (string Name, string Source)[] files)
        {
            lock (Validator)
            {
                Validator.Clear();
                foreach (var (Name, Source) in files)
                {
                    Validator.AddMarkdownFile(Path.Combine(RootDirectory, Name), Source);
                }
                return Validator.ValidateFully();
            }
        }
        public static ValidationReport GetReport(IEnumerable<(string Name, string Source)> files, params string[] entities)
        {
            lock (Validator)
            {
                Validator.Clear();
                foreach (var (Name, Source) in files)
                {
                    Validator.AddMarkdownFile(Path.Combine(RootDirectory, Name), Source);
                }
                foreach (var entity in entities)
                {
                    Validator.AddEntity(Path.Combine(RootDirectory, entity));
                }
                return Validator.ValidateFully();
            }
        }
    }
}
