/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls
{
    public class ExternalLink
    {
        public readonly string Label;
        public readonly Uri Url;

        public readonly bool IsAutoLink;
        public readonly bool IsNamedLinkReference;

        internal readonly SourceSpan Span;
        internal readonly int Line;

        internal ExternalLink(string label, Uri url, SourceSpan span, int line, bool autoLink, bool namedLinkReference)
        {
            Label = label;
            Url = url;
            Span = span;
            Line = line;
            IsAutoLink = autoLink;
            IsNamedLinkReference = namedLinkReference;
        }
    }
}
