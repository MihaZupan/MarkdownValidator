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

        public Dictionary<Type, object> ParserStorage => ParsingResult.ParserStorage;

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
            Source = Object is null
                ? StringSlice.Empty
                : new StringSlice(SourceFile.ParsingResult.Source, Object.Span.Start, Object.Span.End);
        }
        internal void SetWarningSource(WarningSource warningSource)
        {
            WarningSource = warningSource;
        }

        public bool TryGetRelativePath(string reference, out string relativePath)
            => Configuration.TryGetRelativePath(SourceFile.RelativePath, reference, out relativePath);
        public bool TryGetRelativeHtmlPath(string reference, out string relativePath)
            => Configuration.TryGetRelativePath(SourceFile.HtmlPath, reference, out relativePath);

        public bool TryAddReference(string reference, SourceSpan span, int line, bool isImage = false, bool canBeUrl = true, SourceSpan? errorSpan = null)
        {
            if (TryGetRelativePath(reference, out string relativePath))
            {
                ParsingResult.AddReference(
                    new Reference(
                        reference,
                        relativePath,
                        span,
                        line,
                        isImage,
                        canBeUrl));
                return true;
            }
            else
            {
                ReportPathOutOfContext(reference, line, errorSpan ?? span);
                return false;
            }
        }
        public bool TryAddLocalReferenceDefinition(LinkReferenceDefinition referenceDefinition)
        {
            if (TryGetRelativePath(referenceDefinition.Label, out string relativePath))
            {
                ParsingResult.LocalReferenceDefinitions.Add(
                    new ReferenceDefinition(
                        referenceDefinition.Label,
                        relativePath,
                        referenceDefinition.Span,
                        referenceDefinition.Line,
                        SourceFile));
                return true;
            }
            else
            {
                ReportPathOutOfContext(referenceDefinition.Label, referenceDefinition.Line, referenceDefinition.LabelSpan);
                return false;
            }
        }
        private void ReportPathOutOfContext(string path, int line, SourceSpan span)
        {
            ReportWarning(
                WarningID.PathNotInContext,
                line,
                span,
                path,
                "Path `{0}` is not in the validator's working directory",
                path);
        }
        
        public void RegisterAsyncOperation(AsyncProgress progress)
            => ParsingResult.AsyncOperations.AddLast(progress);

        #region Report Warning
        /// <summary>
        /// Report a general warning about the file
        /// </summary>
        public void ReportGeneralWarning(WarningID id, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile), value, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning that applies to the entire parsed object
        /// </summary>
        public void ReportWarning(WarningID id, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, Object.Line, Object.Span, value, messageFormat, messageArgs);

        /// <summary>
        /// Report a warning that applies to the entire reference
        /// </summary>
        internal void ReportWarning(WarningID id, Reference reference, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, reference), reference.RawReference, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        public void ReportWarning(WarningID id, int line, int start, int end, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, line, new SourceSpan(start, end), value, messageFormat, messageArgs);

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        public void ReportWarning(WarningID id, int line, SourceSpan span, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, line, span), value, string.Format(messageFormat, messageArgs));

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
            => Configuration.UrlProcessor.ProcessUrl(this, reference);
    }
}
