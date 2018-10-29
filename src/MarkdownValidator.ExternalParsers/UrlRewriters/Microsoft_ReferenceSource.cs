/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Helpers;
using System;
using System.IO;

namespace MihaZupan.MarkdownValidator.ExternalParsers.UrlRewriters
{
    /// <summary>
    /// Refers to https://referencesource.microsoft.com/scripts.js
    /// </summary>
    public class Microsoft_ReferenceSource : IParser
    {
        public string Identifier => nameof(Microsoft_ReferenceSource);

        public void Initialize(ParserRegistrationContext context)
        {
            context.RegisterUrlRewriter(
                "referencesource.microsoft.com",
                RewriteMicrosoftReferenceSourceUri);
        }

        private Uri RewriteMicrosoftReferenceSourceUri(Uri original)
        {
            string fragment = original.Fragment;
            if (fragment.Length == 0)
                return original;

            string path;
            if (fragment.Contains(',', out int commaIndex))
            {
                path = fragment.Substring(1, commaIndex - 1);
            }
            else path = fragment.Substring(1);

            string extension = Path.GetExtension(path).Substring(1);
            if (SupportedExtensions.ContainsAny(e => e.Equals(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return new Uri(string.Concat("https://referencesource.microsoft.com/", path.Trim(), ".html"));
            }
            else return original;
        }

        private static readonly string[] SupportedExtensions = new string[]
        {
            "cs",
            "vb",
            "ts",
            "csproj",
            "vbproj",
            "targets",
            "props",
            "xaml"
        };
    }
}
