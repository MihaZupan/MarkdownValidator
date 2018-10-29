/*
    Copyright (c) Miha Zupan. All rights reserved.
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
        private readonly Reference Reference;

        public readonly ParsingContext Context;
        public readonly SiteInfo SiteInfo;

        public Uri Url => SiteInfo.Url.Url;
        public Config Configuration => Context.Configuration;
        public WebIOController WebIO => Context.WebIO;

        internal UrlPostProcessorContext(ParsingContext context, Reference reference, SiteInfo siteInfo)
        {
            Context = context;
            SiteInfo = siteInfo;
            Reference = reference;
        }

        internal void ReportWarning(WarningID id, string messageFormat, params string[] messageArgs)
            => Context.ReportWarning(id, Reference, messageFormat, messageArgs);
    }
}
