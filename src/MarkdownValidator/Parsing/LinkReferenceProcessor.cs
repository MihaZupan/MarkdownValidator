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
        private readonly EmailProcessor EmailProcessor;

        public LinkReferenceProcessor(Config configuration)
        {
            Configuration = configuration;
            EmailProcessor = new EmailProcessor(Configuration);
        }

        public void ProcessLinkReference(ParsingContext context, Reference reference)
        {
            if (reference.RawReference.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
            {
                EmailProcessor.ProcessEmail(context, reference);
                return;
            }


        }
    }
}
