/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Threading;

namespace MihaZupan.MarkdownValidator
{
    internal class ValidationContext
    {
        private readonly object _contextLock = new object();
        private void Lock() => Monitor.Enter(_contextLock);
        private void Unlock() => Monitor.Exit(_contextLock);

        private readonly Config Configuration;
        //private readonly ExternalLinkContext ExternalLinkContext;
        private MarkdownPipeline GetNewPipeline()
        {
            var builder = Configuration.PipelineProvider?.GetNewPipeline() ?? new MarkdownPipelineBuilder();

            // Add default extensions
            return builder
                .UsePreciseSourceLocation()
                //.UseAutoIdentifiers(Markdig.Extensions.AutoIdentifiers.AutoIdentifierOptions.None)
                .Build();
        }
        private readonly ParsingController ParsingController;

        // Used to keep track of files, dictionaries and any other user-defined referencable entity in the context
        private readonly HashSet<string> IndexedEntities = new HashSet<string>();

        // Indexed markdown files based on relative paths
        private readonly Dictionary<string, MarkdownFile> IndexedMarkdownFiles = new Dictionary<string, MarkdownFile>();
        private readonly HashSet<MarkdownFile> UnfinishedMarkdownFiles = new HashSet<MarkdownFile>();
        private readonly LinkedList<AsyncProgress> AsyncOperations = new LinkedList<AsyncProgress>();

        // Dictionary names, file names, fragment identifiers and a corresponding list of markdown files that reference them
        // Each reference shouldn't get too many files pointing at it, so using a HashSet here is not necesarry
        // Using a LinkedList here is better, since we don't need indexing, and a LinkedList provides faster removes
        private readonly Dictionary<string, (ReferenceDefinition Reference, LinkedList<MarkdownFile> Files)> ContextReferenceableEntities =
            new Dictionary<string, (ReferenceDefinition Reference, LinkedList<MarkdownFile> Files)>(StringComparer.OrdinalIgnoreCase);

        private void RemoveContextReferencableEntity(string entity)
        {
            var files = ContextReferenceableEntities[entity].Files;
            ContextReferenceableEntities.Remove(entity);
            foreach (var file in files)
            {
                file.ParsingResult.UnprocessedReferences.AddFirst(entity);
                UnfinishedMarkdownFiles.Add(file);
            }
        }

        private void ReparseMarkdownFile(MarkdownFile file)
        {
            UpdateInternalContext(file, file.Update());
        }
        private bool RefreshInternalContext(MarkdownFile file)
        {
            var unprocessedEntities = file.ParsingResult.UnprocessedReferences;
            var node = unprocessedEntities.First;
            while (node != null)
            {
                var next = node.Next;
                if (ContextReferenceableEntities.TryGetValue(node.Value,
                    out (ReferenceDefinition, LinkedList<MarkdownFile> Files) entity))
                {
                    entity.Files.AddLast(file);
                    unprocessedEntities.Remove(node);
                }
                node = next;
            }
            return unprocessedEntities.Count == 0;
        }
        private void AddMarkdownFileToInternalContext(MarkdownFile file)
        {
            foreach (var definedReference in file.ParsingResult.GlobalReferenceDefinitions)
            {
                ContextReferenceableEntities.Add(
                    definedReference.GlobalReference,
                    (definedReference, new LinkedList<MarkdownFile>()));
            }
            foreach (var operation in file.ParsingResult.AsyncOperations)
            {
                AsyncOperations.AddLast(operation);
            }
            UnfinishedMarkdownFiles.Add(file);
        }
        private void RemoveMarkdownFileFromInternalContext(MarkdownFile file)
        {
            foreach (var referenceDefinition in file.ParsingResult.GlobalReferenceDefinitions)
            {
                RemoveContextReferencableEntity(referenceDefinition.GlobalReference);
            }
            foreach (var referencedEntity in file.ParsingResult.References.Keys)
            {
                if (ContextReferenceableEntities.TryGetValue(referencedEntity, out var entity))
                {
                    entity.Files.Remove(file);
                }
            }
            UnfinishedMarkdownFiles.Remove(file);
        }
        private void UpdateInternalContext(MarkdownFile file, ParsingResultGlobalDiff diff)
        {
            foreach (var removedReferenceDefinition in diff.RemovedReferenceDefinitions)
            {
                RemoveContextReferencableEntity(removedReferenceDefinition.GlobalReference);
            }
            foreach (var definedReference in diff.NewReferenceDefinitions)
            {
                ContextReferenceableEntities.Add(
                    definedReference.GlobalReference,
                    (definedReference, new LinkedList<MarkdownFile>()));
            }
            foreach (var referencedEntity in diff.RemovedReferences)
            {
                ContextReferenceableEntities[referencedEntity].Files.Remove(file);
            }
            foreach (var operation in file.ParsingResult.AsyncOperations)
            {
                AsyncOperations.AddLast(operation);
            }
            UnfinishedMarkdownFiles.Add(file);
        }

        #region Public

        public ValidationContext(Config configuration)
        {
            Configuration = configuration;
            ParsingController = new ParsingController(Configuration);
        }

        public bool UpdateMarkdownFile(string path, string source)
        {
            Lock();
            if (IndexedMarkdownFiles.TryGetValue(path, out MarkdownFile markdownFile))
            {
                ParsingResultGlobalDiff diff = markdownFile.Update(source, GetNewPipeline());
                UpdateInternalContext(markdownFile, diff);
                Unlock(); // Lock the entire update context in case there are multiple update calls on the same file
                return true;
            }
            Unlock();
            return false;
        }
        public bool AddMarkdownFile(string fullPath, string path, string source)
        {
            MarkdownFile markdownFile = new MarkdownFile(fullPath, path, source, GetNewPipeline(), ParsingController);
            Lock();
            if (!IndexedMarkdownFiles.ContainsKey(markdownFile.RelativePath))
            {
                if (!IndexedEntities.Contains(markdownFile.RelativePath))
                {
                    IndexedEntities.Add(markdownFile.RelativePath);
                    ContextReferenceableEntities.Add(
                        markdownFile.RelativeLowercasePath,
                        (null, new LinkedList<MarkdownFile>()));
                }
                IndexedMarkdownFiles.Add(markdownFile.RelativePath, markdownFile);
                AddMarkdownFileToInternalContext(markdownFile);
                Unlock();
                return true;
            }
            Unlock();
            return false;
        }
        public bool IndexEntity(string path)
        {
            Lock();
            if (!IndexedEntities.Contains(path))
            {
                IndexedEntities.Add(path);
                ContextReferenceableEntities.Add(
                    path.ToLower(),
                    (null, new LinkedList<MarkdownFile>()));
                Unlock();
                return true;
            }
            Unlock();
            return false;
        }
        public bool RemoveEntityFromIndex(string path, bool isMarkdownFile)
        {
            Lock();
            if (IndexedEntities.Contains(path))
            {
                IndexedEntities.Remove(path);
                if (isMarkdownFile)
                {
                    MarkdownFile markdownFile = IndexedMarkdownFiles[path];
                    IndexedMarkdownFiles.Remove(path);
                    RemoveContextReferencableEntity(markdownFile.RelativeLowercasePath);
                    ContextReferenceableEntities.Remove(markdownFile.RelativeLowercasePath);
                    RemoveMarkdownFileFromInternalContext(markdownFile);
                }
                else
                {
                    RemoveContextReferencableEntity(path.ToLower());
                }
                Unlock();
                return true;
            }
            Unlock();
            return false;
        }
        public void Clear()
        {
            Lock();
            IndexedEntities.Clear();
            IndexedMarkdownFiles.Clear();
            ContextReferenceableEntities.Clear();
            Unlock();
        }

        public ValidationReport Validate(bool getFullReport)
        {
            Lock();
            // Update markdown files for which a re-parsing has been requested by a completed async action
            List<AsyncProgress> finishedProgresses = new List<AsyncProgress>();
            int suggestedWait = -1;
            var node = AsyncOperations.First;
            while (node != null)
            {
                var next = node.Next;
                var progress = node.Value;
                suggestedWait = Math.Max(suggestedWait, progress.SuggestedWait);

                if (!progress.Finished && getFullReport)
                    progress.MRE.WaitOne();

                if (progress.Finished)
                {
                    if (!finishedProgresses.ContainsAny(p => p.File == progress.File))
                    {
                        finishedProgresses.Add(progress);
                        ReparseMarkdownFile(progress.File);
                    }
                    AsyncOperations.Remove(node);
                }
                node = next;
            }

            ValidationReport report = new ValidationReport(AsyncOperations.Count == 0, suggestedWait);

            // Refresh references
            List<MarkdownFile> finishedFiles = new List<MarkdownFile>();
            foreach (var markdownFile in UnfinishedMarkdownFiles)
            {
                if (RefreshInternalContext(markdownFile))
                {
                    if (markdownFile.ParsingResult.ParserWarnings.Count == 0)
                        finishedFiles.Add(markdownFile);
                }
            }
            foreach (var finishedFile in finishedFiles)
                UnfinishedMarkdownFiles.Remove(finishedFile);

            // Find unresolved references
            foreach (var file in UnfinishedMarkdownFiles)
            {
                foreach (var reference in file.ParsingResult.UnprocessedReferences)
                {
                    foreach (var referencePoint in file.ParsingResult.References[reference])
                    {
                        report.AddWarning(
                            WarningID.UnresolvedReference,
                            new WarningLocation(file, referencePoint),
                            "Unresolved reference `{0}`.",
                            referencePoint.RawReference);
                    }
                }
            }

            // Get all parsing warnings
            foreach (var markdownFile in UnfinishedMarkdownFiles)
            {
                report.Warnings.AddRange(markdownFile.ParsingResult.ParserWarnings);
            }

            Unlock();

            return report;
        }

        #endregion Public
    }
}
