/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class ParsingContext
    {
        public Config Configuration => SourceFile.Configuration;
        public string RelativePath => SourceFile.RelativePath;

        public Dictionary<Type, object> ExternalParserStorage => ParsingResult.ExternalParserStorage;

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
            Source = new StringSlice(SourceFile.ParsingResult.Source, Object.Span.Start, Object.Span.End);
        }
        internal void SetWarningSource(WarningSource warningSource)
        {
            WarningSource = warningSource;
        }

        public bool TryGetRelativePath(string reference, out string relativePath)
            => Configuration.TryGetRelativePath(SourceFile.RelativePath, reference, out relativePath);
        public bool TryGetRelativeHtmlPath(string reference, out string relativePath)
            => Configuration.TryGetRelativePath(SourceFile.HtmlPath, reference, out relativePath);

        public void ReportPathOutOfContext(string path, int line, SourceSpan span)
        {
            ReportWarning(
                WarningID.PathNotInContext,
                line,
                span,
                path,
                $"Path `{path}` is not in the validator's working directory");
        }
        
        public void RegisterAsyncOperation(AsyncProgress progress)
            => ParsingResult.AsyncOperations.AddLast(progress);

        #region Report Warning
        /// <summary>
        /// Report a general warning about the file
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        public void ReportGeneralWarning(WarningID id, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile), value, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning that applies to the entire parsed object
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        public void ReportWarning(WarningID id, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, Object.Line, Object.Span, value, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning that applies to the entire reference
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="reference">Reference that the warning is about</param>
        /// <param name="messageFormat">Message or message format string</param>
        /// <param name="messageArgs">Optional arguments for messageFormat</param>
        internal void ReportWarning(WarningID id, Reference reference, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, reference), reference.RawReference, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        /// <param name="id">ID of the warning.</param>
        /// <param name="line">The starting line of the warning span</param>
        /// <param name="span">Span of the source that the warning points at</param>
        /// <param name="message">The message for the warning</param>
        public void ReportWarning(WarningID id, int line, SourceSpan span, string value, string message)
            => ReportWarning(id, new WarningLocation(SourceFile, line, span), value, message);

        private void ReportWarning(WarningID id, WarningLocation location, string value, string message)
            => SourceFile.ParsingResult.ParserWarnings.Add(
                new Warning(
                    id,
                    location,
                    value,
                    message,
                    WarningSource));
        #endregion

        internal void ProcessLinkReference(Reference reference)
            => Configuration.LinkReferenceProcessor.ProcessLinkReference(this, reference);
    }
}
