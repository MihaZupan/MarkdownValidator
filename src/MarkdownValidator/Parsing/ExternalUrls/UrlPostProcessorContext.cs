/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using MihaZupan.MarkdownValidator.WebIO;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls
{
    public class UrlPostProcessorContext
    {
        private readonly LinkReference Reference;

        public readonly ParsingContext Context;
        public readonly SiteInfo SiteInfo;

        public readonly CleanUrl Url;
        public readonly Config Configuration;
        public readonly WebIOController WebIO;

        internal UrlPostProcessorContext(ParsingContext context, LinkReference reference, SiteInfo siteInfo, Uri url)
        {
            Context = context;
            SiteInfo = siteInfo;
            Reference = reference;

            Url = new CleanUrl(url);
            Configuration = Context.Configuration;
            WebIO = Context.WebIO;
        }

        /// <summary>
        /// Report a warning that applies to the entire reference
        /// </summary>
        internal void ReportWarning(WarningID id, string messageFormat, params string[] messageArgs)
            => Context.ReportWarning(id, Reference, messageFormat, messageArgs);
    }
}
