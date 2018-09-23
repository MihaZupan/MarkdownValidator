/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MihaZupan.MarkdownValidator.Warnings;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class LiteralInlineParser : IParser
    {
        public string Identifier => nameof(LiteralInlineParser);

        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LiteralInline), ParseLiteralInline);
        }

        private void ParseLiteralInline(ParsingContext context)
        {
            var inline = context.Object as Inline;
            context.SetWarningSource(WarningSource.InternalParser);

            ReferenceInt lastIndex = GetLastIndex(context);
            if (inline.Span.Start < lastIndex)
                return;

            bool isImage = false;
            bool firstForm = false;
            int start;

            if (inline.Parent is LinkDelimiterInline linkDelimiter)
            {
                // Second-Form = [abc][def]
                isImage = linkDelimiter.IsImage;
                start = linkDelimiter.Span.Start;
                if (linkDelimiter.FirstChild.Span.Start < lastIndex)
                    return;
                inline = linkDelimiter.FirstChild;
            }
            else
            {
                // First-Form = [abc] (could be just '[abc')
                var literal = inline as LiteralInline;
                if (literal.Content.Length == 2 && literal.Content.CurrentChar == '!' && literal.Content.NextChar() == '[')
                    isImage = true;
                else if (literal.Content.Length != 1 || literal.Content.CurrentChar != '[')
                    return;
                inline = inline.NextSibling;
                start = inline?.Span.Start ?? 0;
                firstForm = true;
            }

            bool lineBreaks = false;
            if (!TryExtractReferencePart(ref inline, context.Source.Text, out string name, out SourceSpan nameSpan, out bool canBeUrl, ref lineBreaks))
                return;

            // Update the processed index to avoid parsing the same literals multiple times
            lastIndex.Value = inline.Span.End;

            if (!TryCleanPart(context, ref name, nameSpan, inline.Line, start, firstForm) && firstForm)
                return;

            if (firstForm)
            {
                context.TryAddReference(name, nameSpan, inline.Line, isImage, canBeUrl);
            }
            else
            {
                // The form is [abc][def]
                if (inline.NextSibling is null)
                {
                    context.ReportWarning(
                        WarningIDs.InvalidReferenceNesting,
                        inline.Line, start, lastIndex,
                        string.Empty,
                        "Invalid [] nesting");
                    return;
                }
                if (inline.NextSibling is LineBreakInline newLine)
                {
                    inline = newLine.NextSibling;
                    lineBreaks = true;

                    if (inline.NextSibling is null)
                        return;
                }
                inline = inline.NextSibling.NextSibling;

                int targetStart = inline?.Span.Start ?? 0;
                if (!TryExtractReferencePart(ref inline, context.Source.Text, out string target, out SourceSpan targetSpan, out canBeUrl, ref lineBreaks))
                    return;

                SourceSpan entireSpan = new SourceSpan(start, targetSpan.End);

                // Update the processed index to avoid parsing the same literals multiple times
                lastIndex.Value = inline.Span.End;

                if (TryCleanPart(context, ref target, targetSpan, inline.Line, targetStart, true))
                {
                    if (target.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        context.ReportWarning(
                            WarningIDs.SameLabelAndTargetReference,
                            inline.Line, entireSpan,
                            string.Empty,
                            "You can use `[{0}]` instead of `[{0}][{1}]`",
                            name,
                            target);
                    }
                    context.TryAddReference(target, targetSpan, inline.Line, isImage, canBeUrl);
                }
            }

            if (lineBreaks)
            {
                context.ReportWarning(
                    WarningIDs.ReferenceContainsLineBreak,
                    inline.Line,
                    start, lastIndex,
                    string.Empty,
                    "Ugly :'( Remove those damn line breaks from references!");
            }
        }

        private static bool TryExtractReferencePart(ref Inline inline, string source, out string part, out SourceSpan partSpan, out bool canBeUrl, ref bool lineBreaks)
        {
            var nameBuilder = StringBuilderCache.Local();
            int start = inline?.Span.Start -1 ?? 0;
            canBeUrl = true;

            while (inline != null)
            {
                if (inline is LiteralInline literal)
                {
                    if (literal.Contains(']', out int index))
                    {
                        nameBuilder.Append(literal.Content.Text, literal.Content.Start, index);
                        part = nameBuilder.ToString();
                        inline = literal;
                        partSpan = new SourceSpan(start, literal.Span.Start + index);
                        return true;
                    }
                    nameBuilder.Append(literal.Content);
                }
                else if (inline is LineBreakInline lineBreak)
                {
                    canBeUrl = false;
                    lineBreaks = true;
                    if (inline.NextSibling is LineBreakInline)
                    {
                        break;
                    }
                    nameBuilder.Append(' ');
                }
                else
                {
                    #warning ToDo
                    // Report more info about contained types to further analysis
                    // of what are actual references and what is just weird formatting

                    // Inline emphasis, inline code, inline links ...
                    canBeUrl = false;
                    nameBuilder.Append(source, inline.Span.Start, inline.Span.Length);
                }

                inline = inline.NextSibling;
            }

            partSpan = SourceSpan.Empty;
            part = null;
            return false;
        }
        private static bool TryCleanPart(ParsingContext context, ref string part, SourceSpan partSpan, int line, int start, bool isReference)
        {
            if (part.StartsWith(' '))
            {
                part = part.TrimStart();
                if (part.Length != 0)
                {
                    context.ReportWarning(
                        WarningIDs.ReferenceHasExcessWhitespace,
                        line, partSpan,
                        part,
                        "Leading space in {0} `{1}`",
                        isReference ? "reference" : "link content",
                        part);
                }
            }
            if (part.EndsWith(' '))
            {
                part = part.TrimEnd();
                if (part.Length != 0)
                {
                    context.ReportWarning(
                        WarningIDs.ReferenceHasExcessWhitespace,
                        line, partSpan,
                        part,
                        "Trailing space in {0} `{1}`",
                        isReference ? "reference" : "link content",
                        part);
                }
            }
            if (part.Length == 0)
            {
                context.ReportWarning(
                    isReference ? WarningIDs.EmptyReference : WarningIDs.EmptyLinkContent,
                    line, partSpan,
                    string.Empty,
                    "Empty {0}",
                    isReference ? "reference" : "link content");
                return false;
            }
            return true;
        }

        private static ReferenceInt GetLastIndex(ParsingContext context)
            => context.GetParserState<LinkInlineParser, ReferenceInt>(() => new ReferenceInt(-1));
    }
}
