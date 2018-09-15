/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Text;

namespace MihaZupan.MarkdownValidator.Parsing.Parsers
{
    internal class LiteralInlineParser : IParser
    {
        public void Initialize(ParserRegistrationContext context)
        {
            context.Register(typeof(LiteralInline), ParseLiteralInline);
        }

        private void ParseLiteralInline(ParsingContext context)
        {
            var literal = context.Object as LiteralInline;
            context.SetWarningSource(WarningSource.InternalParser);

            ReferenceInt lastIndex = GetLastIndex(context);
            if (literal.Span.Start <= lastIndex)
                return;

            lastIndex.Value = literal.Span.End;

            bool lineBreaks = false;
            bool isImage = false;
            bool isSecondForm = false;
            int start = literal.Span.Start;

            // First-Form = [abc], Second-Form = [abc][def]
            if (literal.Parent is LinkDelimiterInline linkDelimiter)
            {
                isImage = linkDelimiter.IsImage;
                isSecondForm = true;
                start = linkDelimiter.Span.Start;
            }
            else
            {
                if ((literal.Content.Length == 1 && literal.Content.CurrentChar == '[') ||
                    (literal.Content.Length == 2 && literal.Content.CurrentChar == '!' && literal.Content.NextChar() == '['))
                {
                    isImage = literal.Content.Length == 2;
                    if (literal.NextSibling is LineBreakInline lineBreak)
                    {
                        if (lineBreak.NextSibling is LiteralInline nextSibling)
                        {
                            literal = nextSibling;
                            lineBreaks = true;
                        }
                        else return;
                    }
                    else if (literal.NextSibling is LiteralInline nextSibling)
                    {
                        literal = nextSibling;
                    }
                    else return;
                }
                else return;

                // Update the processed index to avoid parsing the same literals multiple times
                lastIndex.Value = literal.Span.End;
            }

            string name;
            if (!literal.Contains(']', out int localIndex))
            {
                StringBuilder nameBuilder = new StringBuilder();
                do
                {
                    nameBuilder.Append(' ');
                    nameBuilder.Append(literal.Content.ToString());
                    if (literal.NextSibling is LineBreakInline lineBreak)
                    {
                        if (lineBreak.NextSibling is LiteralInline nextSibling)
                        {
                            literal = nextSibling;
                            lineBreaks = true;
                        }
                        else return;
                    }
                    else if (literal.NextSibling is LiteralInline nextSibling)
                    {
                        literal = nextSibling;
                    }
                    else return;
                    // Update the processed index to avoid parsing the same literals multiple times
                    lastIndex.Value = literal.Span.End;
                }
                while (!literal.Contains(']', out localIndex));
                if (localIndex != 0)
                {
                    nameBuilder.Append(' ');
                    nameBuilder.Append(literal.Content.Substring(literal.Content.Start, localIndex));
                }
                name = nameBuilder.ToString();
            }
            else
            {
                name = literal.Content.Substring(literal.Content.Start, localIndex);
            }
            name = name.Trim();
            int end = literal.Span.Start + localIndex;

            if (!isSecondForm)
            {
                // The form is [abc]
                // Make sure it's not empty
                if (name.Length == 0)
                {
                    ReportWarning(
                        context,
                        WarningID.EmptyReference,
                        literal.Line,
                        start, end,
                        "Empty reference");
                }
                else
                {
                    AddReference(context, name, literal.Line, end, isImage);
                }
            }
            else
            {
                // The form is [abc][def]
                if (literal.NextSibling is null)
                {
                    ReportWarning(context, WarningID.InvalidReferenceNesting, literal.Line, start, end, "Invalid [] nesting");
                    return;
                }
                if (literal.NextSibling.NextSibling is LineBreakInline newLine)
                {
                    literal = newLine.NextSibling as LiteralInline;
                    lineBreaks = true;
                }
                else
                {
                    literal = literal.NextSibling.NextSibling as LiteralInline;
                }
                // Update the processed index to avoid parsing the same literals multiple times
                lastIndex.Value = literal.Span.End;

                // The 'abc' part is already stored in name, make sure it's not empty
                if (name.Length == 0)
                {
                    ReportWarning(
                        context,
                        WarningID.EmptyLinkContent,
                        literal.Line,
                        start, end,
                        "Empty link content");
                }

                string target;
                if (!literal.Contains(']', out localIndex))
                {
                    StringBuilder targetBuilder = new StringBuilder();
                    do
                    {
                        targetBuilder.Append(' ');
                        targetBuilder.Append(literal.Content.ToString());
                        if (literal.NextSibling is LineBreakInline lineBreak)
                        {
                            if (lineBreak.NextSibling is LiteralInline nextSibling)
                            {
                                literal = nextSibling;
                                lineBreaks = true;
                            }
                            else return;
                        }
                        else if (literal.NextSibling is LiteralInline nextSibling)
                        {
                            literal = nextSibling;
                        }
                        else return;
                        // Update the processed index to avoid parsing the same literals multiple times
                        lastIndex.Value = literal.Span.End;
                    }
                    while (!literal.Contains(']', out localIndex));
                    if (localIndex != 0)
                    {
                        targetBuilder.Append(' ');
                        targetBuilder.Append(literal.Content.Substring(literal.Content.Start, localIndex));
                    }
                    target = targetBuilder.ToString();
                }
                else
                {
                    target = literal.Content.Substring(literal.Content.Start, localIndex);
                }
                target = target.Trim();
                end = literal.Span.Start + localIndex;

                if (target.Length == 0)
                {
                    ReportWarning(
                        context,
                        WarningID.EmptyReference,
                        literal.Line,
                        start, end,
                        "Empty reference");
                }
                else
                {
                    if (target.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        ReportWarning(
                            context,
                            WarningID.SameLabelAndTargetReference,
                            literal.Line,
                            start, end,
                            string.Format("You can use `[{0}]` instead of `[{0}][{1}]`", name, target));
                    }
                    AddReference(context, target, literal.Line, end, isImage);
                }
            }

            if (lineBreaks)
            {
                ReportWarning(
                    context,
                    WarningID.ReferenceContainsLineBreak,
                    literal.Line,
                    start, end,
                    "Ugly :'( Remove those damn line breaks from references");
            }
        }

        private void ReportWarning(ParsingContext context, WarningID id, int line, int start, int end, string message)
        {
            context.ReportWarning(
                id,
                line,
                new SourceSpan(start, end),
                string.Empty,
                message);
        }
        private void AddReference(ParsingContext context, string reference, int line, int end, bool isImage)
        {
            SourceSpan span = new SourceSpan(context.Object.Span.Start, end);
            if (context.TryGetRelativePath(reference, out string relative))
            {
                context.ParsingResult.AddReference(
                    new Reference(
                        reference,
                        relative,
                        span,
                        line,
                        isImage));
            }
            else
            {
                context.ReportPathOutOfContext(reference, line, span);
            }
        }

        private static readonly Type StorageKey = typeof(LiteralInlineParser);
        private static ReferenceInt GetLastIndex(ParsingContext context)
        {
            if (context.ParserStorage.TryGetValue(StorageKey, out object obj))
            {
                return (ReferenceInt)obj;
            }
            else
            {
                var index = new ReferenceInt(-1);
                context.ParserStorage.Add(StorageKey, index);
                return index;
            }
        }
    }
}
