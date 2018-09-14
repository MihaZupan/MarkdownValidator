/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal sealed class LinkReferenceProcessor
    {
        private readonly Config Configuration;

        public LinkReferenceProcessor(Config configuration)
        {
            Configuration = configuration;
        }

        public void ProcessLinkReference(ParsingContext context, Reference reference)
        {
            if (reference.RawReference.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                ProcessEmail(context, reference);
            }
            else
            {
                ProcessLink(context, reference);
            }
        }

        private void ProcessEmail(ParsingContext context, Reference reference)
        {
            string email = reference.RawReference.Substring(7);
            if (email.StartsWith('@') || email.EndsWith('@'))
            {
                context.ReportWarning(
                    WarningID.InvalidEmailFormat,
                    reference,
                    "`{0}` is not a valid email",
                    email);
                return;
            }

            string host = email.SubstringAfter('@');
            if (HostnameHelper.IsDocumentationHostname(host))
                return;

            Console.WriteLine("Host: " + host);
        }

        private void ProcessLink(ParsingContext context, Reference reference)
        {

        }
    }
}
