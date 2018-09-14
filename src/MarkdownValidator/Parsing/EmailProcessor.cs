/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal sealed class EmailProcessor
    {
        private readonly Config Configuration;

        public EmailProcessor(Config configuration)
        {
            Configuration = configuration;
        }

        public void ProcessEmail(ParsingContext context, Reference reference)
        {
            string email = reference.RawReference.Substring(7); // mailto:
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


        }
    }
}
