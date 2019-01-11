/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Helpers;
using System;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class LinkReference : Reference, IEquatable<LinkReference>
    {
        public readonly string Label;
        public Uri Url; // Set in UrlProcessor

        public readonly bool IsImage;
        public readonly bool IsCleanUrl;
        public readonly bool IsAutoLink;
        public readonly bool IsNamedReferenceLink;

        public LinkReference(string reference, SourceSpan span, int line, bool image, bool cleanUrl, bool autoLink, bool namedReferenceLink, string label = null)
            : base(reference, reference, span, line)
        {
            Label = label;
            IsImage = image;
            IsCleanUrl = cleanUrl;
            IsAutoLink = autoLink;
            IsNamedReferenceLink = namedReferenceLink;
        }

        public override int GetHashCode()
        {
            int hashCode = base.GetHashCode() * 397;
            if (IsImage) hashCode += 1;
            if (IsCleanUrl) hashCode += 2;
            if (IsAutoLink) hashCode += 4;
            if (IsNamedReferenceLink) hashCode += 8;
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is LinkReference link)
            {
                return Equals(link);
            }
            return false;
        }
        public bool Equals(LinkReference other)
        {
            if (!other.Equals(this as Reference)) return false;
            return other.IsImage == IsImage &&
                other.IsCleanUrl == IsCleanUrl &&
                other.IsAutoLink == IsAutoLink &&
                other.IsNamedReferenceLink == IsNamedReferenceLink &&
                other.Label.OrdinalEquals(Label) &&
                (other.Url?.AbsoluteUri).OrdinalEquals(Url?.AbsoluteUri);
        }
    }
}
