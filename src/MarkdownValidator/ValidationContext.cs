/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MihaZupan.MarkdownValidator
{
    internal class ValidationContext
    {
        private readonly object _contextLock = new object();
        private void Lock() => Monitor.Enter(_contextLock);
        private void Unlock() => Monitor.Exit(_contextLock);

        private readonly Config Configuration;

        // Used to keep track of files, dictionaries and any other user-defined referencable entity in the context
        private readonly HashSet<string> IndexedEntities =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Indexed markdown files based on relative paths
        private readonly Dictionary<string, MarkdownFile> IndexedMarkdownFiles =
            new Dictionary<string, MarkdownFile>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<MarkdownFile> UnfinishedMarkdownFiles =
            new HashSet<MarkdownFile>();
        private readonly Dictionary<MarkdownFile, LinkedList<AsyncProgress>> AsyncOperations =
            new Dictionary<MarkdownFile, LinkedList<AsyncProgress>>();

        // Dictionary names, file names, fragment identifiers and a corresponding list of markdown files that reference them
        // Each reference shouldn't get too many files pointing at it, so using a HashSet here is not necesarry
        // Using a LinkedList here is better, since we don't need indexing, and a LinkedList provides faster removes
        private readonly Dictionary<string, (ReferenceDefinition Reference, LinkedList<MarkdownFile> Files)> ContextReferenceableEntities =
            new Dictionary<string, (ReferenceDefinition Reference, LinkedList<MarkdownFile> Files)>(StringComparer.OrdinalIgnoreCase);

        private void RemoveContextReferencableEntity(string entity, MarkdownFile ignoreFile = null)
        {
            var files = ContextReferenceableEntities[entity].Files;
            ContextReferenceableEntities.Remove(entity);
            foreach (var file in files)
            {
                if (file == ignoreFile)
                    continue;

                file.ParsingResult.UnprocessedReferences.AddFirst(entity);
                UnfinishedMarkdownFiles.Add(file);
            }
        }
        private void AddAsyncOperations(MarkdownFile file)
        {
            if (file.ParsingResult.AsyncOperations.Count == 0)
                return;

            if (AsyncOperations.TryGetValue(file, out LinkedList<AsyncProgress> asyncOperations))
            {
                foreach (var asyncOperation in file.ParsingResult.AsyncOperations)
                {
                    asyncOperations.AddLast(asyncOperation);
                }
            }
            else
            {
                AsyncOperations.Add(file, file.ParsingResult.AsyncOperations);
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
            if (!IndexedEntities.Contains(file.RelativePath))
            {
                IndexedEntities.Add(file.RelativePath);
                ContextReferenceableEntities.Add(
                    file.RelativePath,
                    (null, new LinkedList<MarkdownFile>()));
            }

            // We have to do this check in case markdown files don't actually end with .md, but share the name
            if (!ContextReferenceableEntities.ContainsKey(file.HtmlPath))
            {
                ContextReferenceableEntities.Add(
                    file.HtmlPath,
                    (null, new LinkedList<MarkdownFile>()));
            }

            foreach (var definedReference in file.ParsingResult.HeadingDefinitions)
            {
                ContextReferenceableEntities.Add(
                    definedReference.GlobalReference,
                    (definedReference, new LinkedList<MarkdownFile>()));
            }

            AddAsyncOperations(file);
            UnfinishedMarkdownFiles.Add(file);
        }
        private void RemoveMarkdownFileFromInternalContext(MarkdownFile file)
        {
            RemoveContextReferencableEntity(file.RelativePath);
            ContextReferenceableEntities.Remove(file.RelativePath);
            ContextReferenceableEntities.Remove(file.HtmlPath);

            foreach (var referenceDefinition in file.ParsingResult.HeadingDefinitions)
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
            AsyncOperations.Remove(file);
            UnfinishedMarkdownFiles.Remove(file);
        }
        private void UpdateInternalContext(MarkdownFile file, ParsingResultGlobalDiff diff)
        {
            foreach (var removedHeadingDefinition in diff.RemovedHeadingDefinitions)
            {
                RemoveContextReferencableEntity(removedHeadingDefinition.GlobalReference, file);
            }
            foreach (var definedHeading in diff.NewHeadingDefinitions)
            {
                ContextReferenceableEntities.Add(
                    definedHeading.GlobalReference,
                    (definedHeading, new LinkedList<MarkdownFile>()));
            }
            foreach (var referencedEntity in diff.RemovedReferences)
            {
                // Do an exists check here - would be the same if we excluded removed references
                // that existed in UnprocessedReferences when creating the ParsingResultGlobalDiff
                if (ContextReferenceableEntities.TryGetValue(referencedEntity, out var references))
                {
                    references.Files.Remove(file);
                }
            }

            AddAsyncOperations(file);
            UnfinishedMarkdownFiles.Add(file);
        }

        #region Public

        public ValidationContext(Config configuration)
        {
            Configuration = configuration;
            Configuration.Initialize();
            ContextReferenceableEntities.Add(string.Empty, (null, new LinkedList<MarkdownFile>()));
        }

        public bool UpdateMarkdownFile(string path, string source)
        {
            Lock();
            if (IndexedMarkdownFiles.TryGetValue(path, out MarkdownFile markdownFile))
            {
                ParsingResultGlobalDiff diff = markdownFile.Update(source);
                UpdateInternalContext(markdownFile, diff);
                Unlock(); // Lock the entire update context in case there are multiple update calls on the same file
                return true;
            }
            Unlock();
            return false;
        }
        public bool AddMarkdownFile(string fullPath, string relativePath, string source)
        {
            MarkdownFile markdownFile = new MarkdownFile(fullPath, relativePath, source, Configuration);
            Lock();
            if (!IndexedMarkdownFiles.ContainsKey(markdownFile.RelativePath))
            {
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
                    path,
                    (null, new LinkedList<MarkdownFile>()));
                Unlock();
                return true;
            }
            Unlock();
            return false;
        }
        public bool RemoveEntityFromIndex(string path)
        {
            Lock();
            if (IndexedEntities.Contains(path))
            {
                IndexedEntities.Remove(path);
                if (IndexedMarkdownFiles.TryGetValue(path, out MarkdownFile file))
                {
                    IndexedMarkdownFiles.Remove(path);
                    RemoveMarkdownFileFromInternalContext(file);
                }
                else
                {
                    RemoveContextReferencableEntity(path);
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
            AsyncOperations.Clear();
            IndexedEntities.Clear();
            IndexedMarkdownFiles.Clear();
            UnfinishedMarkdownFiles.Clear();
            ContextReferenceableEntities.Clear();
            Unlock();
        }

        public ValidationReport Validate(bool getFullReport, CancellationToken cancellationToken = default)
        {
            Lock();

            // Check async operations and reparse updated files
            int suggestedWait = -1;
            List<MarkdownFile> filesToReparse = new List<MarkdownFile>();
            foreach (var asyncOperations in AsyncOperations.Values)
            {
                bool reparse = false;
                var node = asyncOperations.First;
                MarkdownFile file = node.Value.File;

                while (node != null)
                {
                    var next = node.Next;
                    var progress = node.Value;
                    suggestedWait = Math.Max(suggestedWait, progress.SuggestedWait);

                    if (!progress.Finished && getFullReport)
                    {
                        if (cancellationToken == default) progress.MRE.WaitOne();
                        else WaitHandle.WaitAny(new[] { progress.MRE, cancellationToken.WaitHandle });
                    }

                    if (progress.Finished)
                    {
                        reparse = true;
                        asyncOperations.Remove(node);
                    }
                    node = next;
                }

                if (reparse)
                    filesToReparse.Add(file);
            }
            foreach (var file in filesToReparse)
            {
                ReparseMarkdownFile(file);
                if (AsyncOperations[file].Count == 0)
                    AsyncOperations.Remove(file);
            }

            ValidationReport report = new ValidationReport(Configuration, AsyncOperations.Count == 0, suggestedWait);

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
                    if (reference.StartsWith('^'))
                    {
                        foreach (var referencePoint in file.ParsingResult.References[reference])
                        {
                            report.AddWarning(
                                WarningIDs.UnresolvedFootnoteReference,
                                new WarningLocation(file, referencePoint),
                                referencePoint.RawReference,
                                "Unresolved footnote reference `{0}`",
                                referencePoint.RawReference);
                        }
                    }
                    else
                    {
                        foreach (var referencePoint in file.ParsingResult.References[reference])
                        {
                            report.AddWarning(
                                WarningIDs.UnresolvedReference,
                                new WarningLocation(file, referencePoint),
                                referencePoint.RawReference,
                                "Unresolved reference `{0}`",
                                referencePoint.RawReference);
                        }
                    }
                }
            }

            // Get all parsing warnings
            foreach (var markdownFile in UnfinishedMarkdownFiles)
            {
                report.AddWarnings(markdownFile.ParsingResult.ParserWarnings);
            }

            Unlock();

            return report;
        }

        #endregion Public
    }
}
