/*
    Copyright (c) 2019 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using MihaZupan.MarkdownValidator.WebIO;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls.UrlPostProcessors.GitHub
{
    internal class GitHubUrlPostProcessor : IParser
    {
        public string Identifier => nameof(GitHubUrlPostProcessor);

        public void Initialize(ParserRegistrationContext context)
        {
            context.RegisterUrlPostProcessor(
                "github.com",
                ProcessGithubLinks);

            context.RegisterDownloadableContent(
                "raw.githubusercontent.com",
                "text/plain",
                isText: true);
        }

        private void ProcessGithubLinks(UrlPostProcessorContext context)
        {
            string label = context.Label;
            if (label == null || label.Length == 0 || context.IsAutoLink)
                return;

            string fragment = context.UrlFragment; // #L42 or #L42-L100
            if (fragment.Length < 3)
                return;

            if (char.ToUpper(fragment[1]) != 'L')
                return;

            if (fragment.Contains('-', 2)) // Should we ignore ranges?
                return;

            if (!int.TryParse(fragment.Substring(2), out int lineNumber) || lineNumber == 0)
                return;

            if (!GitHubRawUserContent.TryGetRawUserContentUrl(context.Url, out Uri rawContentUrl))
                return;

            string cleanLabel = label.Trim('*', '_', '`');
            if (cleanLabel.Length == 0)
                return;

            var state = context.WebIO.TryGetSiteInfo(rawContentUrl, out SiteInfo siteInfo);
            if (state == SiteCacheState.Unknown) context.Context.RegisterPendingOperation(context.WebIO.RequestSiteInfo(rawContentUrl));
            if (state != SiteCacheState.Cached) return;

            string source = siteInfo.ContentText;

            int lineIndex = 0;
            int foundLineNumber = 0;
            while (++foundLineNumber < lineNumber)
            {
                lineIndex = source.IndexOf('\n', lineIndex + 1);
                if (lineIndex == -1 || lineIndex == source.Length - 1)
                    break;
            }

            if (foundLineNumber == lineNumber)
            {
                int endOfLine = source.IndexOf('\n', ++lineIndex);
                if (endOfLine != -1)
                {
                    if (source.IndexOf(cleanLabel, lineIndex, endOfLine - lineIndex, StringComparison.OrdinalIgnoreCase) != -1)
                        return; // We found the label at the target line
                }
            }

            // We did not find the line number or the value at the line number
            context.ReportWarning(
                WarningIDs.NoLabelOnGitHubLineUrl,
                "Could not find a reference to `{0}` on line {1}",
                cleanLabel,
                lineNumber.ToString());
        }
    }
}
