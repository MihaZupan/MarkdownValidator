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
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using MihaZupan.MarkdownValidator.WebIO;
using System;
using static MihaZupan.MarkdownValidator.Helpers.PathHelper;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class ParsingContext
    {
        public Config Configuration => SourceFile.Configuration;
        public string RelativePath => SourceFile.RelativePath;

        public StringSlice GetSourceLine()
        {
            int line = Object.Line;
            int startIndex = ParsingResult.LineStartIndexes[line];
            int endIndex;

            if (ParsingResult.LineStartIndexes.Count == line + 1)
            {
                endIndex = Source.Length;
            }
            else
            {
                endIndex = ParsingResult.LineStartIndexes[line + 1];
            }

            return Source.Reposition(startIndex, endIndex);
        }

        public ObjectType GetParserState<ParserType, ObjectType>(Func<ObjectType> init)
        {
            if (!ParsingResult.ParserStorage.TryGetValue(typeof(ParserType), out object state))
            {
                state = init();
                ParsingResult.ParserStorage.Add(typeof(ParserType), state);
            }
            return (ObjectType)state;
        }

        // Only available to internal parsers
        internal readonly MarkdownFile SourceFile;
        internal ParsingResult ParsingResult => SourceFile.ParsingResult;

        public MarkdownObject Object { get; private set; }
        public StringSlice Source { get; private set; }
        public WarningSource WarningSource { get; private set; }
        public string ParserIdentifier { get; private set; }
        public readonly WebIOController WebIO;

        internal ParsingContext(MarkdownFile sourceFile)
        {
            SourceFile = sourceFile;
            WebIO = Configuration.WebIO.WebIOController;
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
        internal void SetWarningSource(WarningSource warningSource, string parserIdentifier)
        {
            WarningSource = warningSource;
            ParserIdentifier = parserIdentifier;
        }

        internal PathProcessingResult ProcessRelativePath(string reference, out string relativePath)
            => Configuration.PathHelper.ProcessRelativePath(SourceFile.RelativePath, reference, out relativePath);
        internal PathProcessingResult ProcessRelativeHtmlPath(string reference, out string relativePath)
            => Configuration.PathHelper.ProcessRelativePath(SourceFile.HtmlPath, reference, out relativePath);

        public bool TryAddReference(string reference, SourceSpan span, int line, bool isImage = false, bool canBeUrl = true, SourceSpan? errorSpan = null)
        {
            var result = ProcessRelativePath(reference, out string relativePath);

            if (result == PathProcessingResult.NotInContext)
            {
                ReportPathOutOfContext(reference, errorSpan ?? span);
                return false;
            }
            else if (result == PathProcessingResult.IsFsSpecific)
            {
                ReportPathIsFsSpecific(reference, errorSpan ?? span);
                return false;
            }
            else
            {
                bool isUrl = result == PathProcessingResult.IsUrl;
                ParsingResult.AddReference(
                    new Reference(
                        reference,
                        isUrl ? reference : relativePath,
                        span,
                        line,
                        isImage,
                        isUrl,
                        isUrl && canBeUrl));
                return true;
            }
        }
        public void AddLocalReferenceDefinition(LinkReferenceDefinition referenceDefinition)
        {
            ParsingResult.LocalReferenceDefinitions.Add(
                new ReferenceDefinition(
                    referenceDefinition.Label,
                    referenceDefinition.Label,
                    referenceDefinition.Span,
                    referenceDefinition.Line,
                    SourceFile));

            TryAddReference(
                referenceDefinition.Url,
                referenceDefinition.Span,
                referenceDefinition.Line,
                errorSpan: referenceDefinition.UrlSpan);
        }
        private void ReportPathOutOfContext(string path, SourceSpan span)
        {
            ReportWarning(
                WarningIDs.PathNotInContext,
                span,
                path,
                "Path `{0}` is not in the validator's working directory",
                path);
        }
        private void ReportPathIsFsSpecific(string path, SourceSpan span)
        {
            ReportWarning(
                WarningIDs.PathIsFsSpecific,
                span,
                path,
                "`{0}` is not a relative path to the markdown context",
                path);
        }
        
        public void RegisterPendingOperation(PendingOperation operation)
        {
            operation.File = SourceFile;
            ParsingResult.PendingOperations.AddLast(operation);
        }

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
            => ReportWarning(id, Object.Span, value, messageFormat, messageArgs);

        /// <summary>
        /// Report a warning that applies to the entire reference
        /// </summary>
        internal void ReportWarning(WarningID id, Reference reference, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, reference), reference.RawReference, string.Format(messageFormat, messageArgs));

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        public void ReportWarning(WarningID id, int start, int end, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new SourceSpan(start, end), value, messageFormat, messageArgs);

        /// <summary>
        /// Report a warning about a specific span of source
        /// </summary>
        public void ReportWarning(WarningID id, SourceSpan span, string value, string messageFormat, params string[] messageArgs)
            => ReportWarning(id, new WarningLocation(SourceFile, span), value, string.Format(messageFormat, messageArgs));

        private void ReportWarning(WarningID id, WarningLocation location, string value, string message)
            => SourceFile.ParsingResult.ParserWarnings.Add(
                new Warning(
                    id,
                    location,
                    value,
                    message,
                    WarningSource,
                    ParserIdentifier));
        #endregion

        internal void ProcessLinkReference(Reference reference)
            => Configuration.WebIO.UrlProcessor.ProcessUrl(this, reference);
    }
}
