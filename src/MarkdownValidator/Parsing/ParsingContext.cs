﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public class ParsingContext
    {
        public Config Configuration => SourceFile.Configuration;
        public string RelativePath => SourceFile.RelativePath;

        // Only available to internal parsers
        internal readonly MarkdownFile SourceFile;
        internal ParsingResult ParsingResult => SourceFile.ParsingResult;

        public MarkdownObject Object { get; private set; }
        public StringSlice Source { get; private set; }
        public WarningSource WarningSource { get; private set; }

        internal ParsingContext(MarkdownFile sourceFile)
        {
            SourceFile = sourceFile;
        }
        internal void Update(MarkdownObject objectToParse)
        {            
            Object = objectToParse;
            Source = new StringSlice(SourceFile.ParsingResult.Source, objectToParse.Span.Start, objectToParse.Span.End);
        }
        internal void SetWarningSource(WarningSource warningSource)
        {
            WarningSource = warningSource;
        }

        public string GetRelativePath(string reference)
            => Configuration.GetRelativePath(SourceFile.RelativePath, reference);
        
        public void RegisterAsyncOperation(AsyncProgress progress)
            => ParsingResult.AsyncOperations.Add(progress);

        /// <summary>
        /// Report a general warning about the file
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        public void ReportGeneralWarning(WarningID id, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile), string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning that applies to the entire parsed object
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        public void ReportWarning(WarningID id, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, Object.Line, Object.Span, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning that applies to the entire reference
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="reference">Reference that the warning is about</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        internal void ReportWarning(WarningID id, Reference reference, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, reference), string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="line">The starting line of the warning span</param>
        /// <param name="span">Span of the source that the warning points at</param>
        /// <param name="message">The message for the warning</param>
        public void ReportWarning(WarningID id, int line, SourceSpan span, string message)
            => ReportWarning(id, new WarningLocation(SourceFile, line, span), message);

        private void ReportWarning(WarningID id, WarningLocation location, string message)
            => SourceFile.ParsingResult.ParserWarnings.Add(
                new Warning(
                    id,
                    location,
                    message,
                    WarningSource));
    }
}
