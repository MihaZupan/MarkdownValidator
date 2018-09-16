/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
namespace MihaZupan.MarkdownValidator.Warnings
{
    /// <summary>
    /// All <see cref="WarningID"/>s with values higher than <see cref="Error"/> (1M) are considered errors
    /// <para>All <see cref="WarningID"/>s with values lower than <see cref="Warning"/> (0) are considered suggestions</para>
    /// </summary>
    public enum WarningID
    {
        //
        // Suggestions
        //
        Suggestion = -1000000,

        SameLabelAndTargetReference,
        HugeMarkdownFile,
        UrlHostnameIsIP,

        //
        // Warnings
        //
        Warning = 0,

        EmptyMarkdownFile,
        UnusedDefinedReference,
        UnusedDefinedFootnote,
        EmptyCodeBlock,
        EmptyHeading,
        EmptyReference,
        EmptyLinkContent,
        HeadingEndsWithWhitespace,
        ReferencePartHasExcessWhitespace,

        //
        // Errors
        //
        Error = 1000000,

        PathNotInContext,
        UnresolvedReference,
        UnresolvedFootnoteReference,
        DuplicateReferenceDefinition,
        DuplicateHeadingDefinition,
        InvalidEmailFormat, // Unused
        InvalidUrlFormat,
        ReferenceContainsLineBreak,
        InvalidReferenceNesting,
    }
}
