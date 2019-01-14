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
using System.Diagnostics;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls.UrlPostProcessors
{
    internal class TelegramBotApiDocsUrlPostProcessor : IParser
    {
        public string Identifier => nameof(TelegramBotApiDocsUrlPostProcessor);

        public void Initialize(ParserRegistrationContext context)
        {
            context.RegisterUrlPostProcessor(
                "core.telegram.org",
                ProcessTelegramBotApiDocs);

            context.RegisterDownloadableContent(
                "core.telegram.org",
                "text/html",
                isText: true,
                IsDocumentationUrl);
        }

        private bool IsDocumentationUrl(CleanUrl url)
        {
            return url.AbsoluteUrlWithoutFragment.EndsWith("core.telegram.org/bots/api", StringComparison.OrdinalIgnoreCase);
        }

        private void ProcessTelegramBotApiDocs(UrlPostProcessorContext context)
        {
            if (!IsDocumentationUrl(context.Url))
                return;

            Debug.Assert(context.SiteInfo.ContentPresent && context.SiteInfo.ContentIsText);

            string fragment = context.Url.Url.Fragment;
            if (fragment.Length == 0)
                return;

            if (!context.SiteInfo.ContentText.OrdinalContains(fragment))
            {
                context.ReportWarning(
                    WarningIDs.InvalidUrlFragment,
                    "Fragment `{0}` is not present on the documentation page",
                    fragment);
            }
        }
    }
}
