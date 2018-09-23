/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
namespace MihaZupan.MarkdownValidator.Warnings
{
    public static class WarningIDs
    {
        //
        // Suggestions
        //
        public static readonly WarningID EmptyContext                   = new WarningID(-1, nameof(EmptyContext));
        public static readonly WarningID SameLabelAndTargetReference    = new WarningID(-2, nameof(SameLabelAndTargetReference));
        public static readonly WarningID HugeMarkdownFile               = new WarningID(-3, nameof(HugeMarkdownFile));
        public static readonly WarningID UrlHostnameIsIP                = new WarningID(-4, nameof(UrlHostnameIsIP));

        //
        // Warnings
        //
        public static readonly WarningID EmptyMarkdownFile              = new WarningID(0,  nameof(EmptyMarkdownFile));
        public static readonly WarningID UnusedDefinedReference         = new WarningID(1,  nameof(UnusedDefinedReference));
        public static readonly WarningID UnusedDefinedFootnote          = new WarningID(2,  nameof(UnusedDefinedFootnote));
        public static readonly WarningID EmptyCodeBlock                 = new WarningID(3,  nameof(EmptyCodeBlock));
        public static readonly WarningID EmptyHeading                   = new WarningID(4,  nameof(EmptyHeading));
        public static readonly WarningID EffectivelyEmptyHeading        = new WarningID(5,  nameof(EffectivelyEmptyHeading));
        public static readonly WarningID EmptyReference                 = new WarningID(6,  nameof(EmptyReference));
        public static readonly WarningID EmptyLinkContent               = new WarningID(7,  nameof(EmptyLinkContent));
        public static readonly WarningID EmptyFootnoteDefinition        = new WarningID(8,  nameof(EmptyFootnoteDefinition));
        public static readonly WarningID HeadingEndsWithWhitespace      = new WarningID(9,  nameof(HeadingEndsWithWhitespace));
        public static readonly WarningID ReferenceHasExcessWhitespace   = new WarningID(10, nameof(ReferenceHasExcessWhitespace));

        //
        // Errors
        //
        public const int ErrorID = 1000000;
        public static readonly WarningID PathNotInContext               = new WarningID(ErrorID,        nameof(PathNotInContext));
        public static readonly WarningID UnresolvedReference            = new WarningID(ErrorID + 1,    nameof(UnresolvedReference));
        public static readonly WarningID UnresolvedFootnoteReference    = new WarningID(ErrorID + 2,    nameof(UnresolvedFootnoteReference));
        public static readonly WarningID DuplicateReferenceDefinition   = new WarningID(ErrorID + 3,    nameof(DuplicateReferenceDefinition));
        public static readonly WarningID DuplicateHeadingDefinition     = new WarningID(ErrorID + 4,    nameof(DuplicateHeadingDefinition));
        public static readonly WarningID InvalidEmailFormat             = new WarningID(ErrorID + 5,    nameof(InvalidEmailFormat));
        public static readonly WarningID InvalidUrlFormat               = new WarningID(ErrorID + 6,    nameof(InvalidUrlFormat));
        public static readonly WarningID ReferenceContainsLineBreak     = new WarningID(ErrorID + 7,    nameof(ReferenceContainsLineBreak));
        public static readonly WarningID InvalidReferenceNesting        = new WarningID(ErrorID + 8,    nameof(InvalidReferenceNesting));
    }
}
