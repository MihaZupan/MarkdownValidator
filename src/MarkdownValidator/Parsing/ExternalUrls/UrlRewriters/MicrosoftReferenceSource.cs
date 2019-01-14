/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Helpers;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls.UrlRewriters
{
    /// <summary>
    /// Refers to https://referencesource.microsoft.com/scripts.js
    /// </summary>
    internal class MicrosoftReferenceSourceUrlRewriter : IParser
    {
        public string Identifier => nameof(MicrosoftReferenceSourceUrlRewriter);

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

            if (path.EndsWithAny(StringComparison.OrdinalIgnoreCase, SupportedExtensions))
            {
                return new Uri(string.Concat("https://referencesource.microsoft.com/", path.Trim(), ".html"));
            }
            else return original;
        }

        private static readonly string[] SupportedExtensions = new string[]
        {
            ".cs",
            ".vb",
            ".ts",
            ".csproj",
            ".vbproj",
            ".targets",
            ".props",
            ".xaml"
        };
    }
}
