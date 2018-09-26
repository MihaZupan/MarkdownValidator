/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls
{
    internal sealed class UrlProcessor
    {
        private readonly Config Configuration;

        public UrlProcessor(Config configuration)
        {
            Configuration = configuration;
        }

        public void ProcessUrl(ParsingContext context, Reference reference)
        {
            string urlString = reference.RawReference;

            if (urlString.StartsWith(':') ||
                urlString.EndsWith(':') ||
                !Uri.TryCreate(urlString, UriKind.RelativeOrAbsolute, out Uri url))
            {
                context.ReportWarning(
                    WarningIDs.InvalidUrlFormat,
                    reference,
                    "`{0}` is not a valid url",
                    urlString);
                return;
            }

            if (url.IsAbsoluteUri && HostnameHelper.IsDocumentationHostname(url.Host))
                return;

            if (!Uri.TryCreate(urlString, UriKind.Absolute, out url))
            {
                context.ReportWarning(
                    WarningIDs.InvalidUrlFormat,
                    reference,
                    "`{0}` is not a valid url",
                    urlString);
                return;
            }

            if (url.HostNameType == UriHostNameType.IPv4 ||
                url.HostNameType == UriHostNameType.IPv6)
            {
                context.ReportWarning(
                    WarningIDs.UrlHostnameIsIP,
                    reference,
                    "Use a hostname instead of an IP address");
            }

            // ToDo: Extensible scheme parsing support
            if (url.Scheme.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                ProcessHttpUrl(context, reference, url);
            }
        }

        private void ProcessHttpUrl(ParsingContext context, Reference reference, Uri url)
        {
            #warning ToDo
        }
    }
}
