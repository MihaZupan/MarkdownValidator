/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Parsing;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private readonly Dictionary<MarkdownFile, LinkedList<PendingOperation>> PendingOperations =
            new Dictionary<MarkdownFile, LinkedList<PendingOperation>>();

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
        private void AddPendingOperations(MarkdownFile file, bool clearFirst = false)
        {
            if (clearFirst)
                PendingOperations.Remove(file);

            if (file.ParsingResult.PendingOperations.Count == 0)
                return;

            if (PendingOperations.TryGetValue(file, out LinkedList<PendingOperation> operations))
            {
                foreach (var operation in file.ParsingResult.PendingOperations)
                {
                    operations.AddLast(operation);
                }
            }
            else
            {
                PendingOperations.Add(file, file.ParsingResult.PendingOperations);
            }
        }

        private void ReparseMarkdownFile(MarkdownFile file)
        {
            UpdateInternalContext(file, file.Update(), true);
        }
        private bool RefreshInternalContext(MarkdownFile file, ValidationReport report)
        {
            var unprocessedEntities = file.ParsingResult.UnprocessedReferences;
            var node = unprocessedEntities.First;
            while (node != null)
            {
                var next = node.Next;
                bool allReferencesAreGood = true;

                if (ContextReferenceableEntities.TryGetValue(node.Value,
                    out (ReferenceDefinition Definition, LinkedList<MarkdownFile> Files) entity))
                {
                    if (entity.Definition is null)
                    {
                        // This is a reference to a file/directory that seems to exist in the context
                        // Check for case equality - OrdinalIgnoreCase matched -> try Ordinal
                        IndexedEntities.TryGetValue(node.Value, out string originalName);
                        foreach (var reference in file.ParsingResult.References[node.Value])
                        {
                            if (!originalName.OrdinalEquals(reference.GlobalReference))
                            {
                                // The referenced file is not an exact case-match for the file on disk
                                // That is a possible problem when using *nix hosts with case-sensitive file systems
                                allReferencesAreGood = false;
                                report.AddWarning(
                                    WarningIDs.FileReferenceCaseMismatch,
                                    new WarningLocation(file, reference),
                                    reference.GlobalReference,
                                    WarningSource.RefreshInternalContext,
                                    "`{0}` is not an exact case-match for `{1}`. This could be a problem on *nix hosts.",
                                    reference.GlobalReference, originalName);
                            }
                        }
                    }

                    if (allReferencesAreGood)
                    {
                        // All is well, case-sensitive equality confirmed
                        entity.Files.AddLast(file);
                        unprocessedEntities.Remove(node);
                    }
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
                IndexedEntities.Add(file.HtmlPath);
            }

            foreach (var definedReference in file.ParsingResult.HeadingDefinitions)
            {
                ContextReferenceableEntities.Add(
                    definedReference.GlobalReference,
                    (definedReference, new LinkedList<MarkdownFile>()));
            }

            AddPendingOperations(file);
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
            PendingOperations.Remove(file);
            UnfinishedMarkdownFiles.Remove(file);
        }
        private void UpdateInternalContext(MarkdownFile file, ParsingResultGlobalDiff diff, bool reparsing = false)
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

            AddPendingOperations(file, !reparsing);
            UnfinishedMarkdownFiles.Add(file);
        }

        private void TryProcessPendingOperations(bool getFullReport, CancellationToken cancellationToken)
        {
            // Check async operations and reparse updated files
            List<MarkdownFile> filesToReparse = new List<MarkdownFile>();
            foreach (var operations in PendingOperations.Values)
            {
                bool reparse = false;
                var node = operations.First;
                MarkdownFile file = node.Value.File;
                Debug.Assert(file.ParsingResult.SyntaxTree != null);

                while (node != null)
                {
                    var next = node.Next;
                    var operation = node.Value;

                    if (!operation.Finished && getFullReport)
                    {
                        if (cancellationToken == default) operation.MRE.WaitOne();
                        else WaitHandle.WaitAny(new[] { operation.MRE, cancellationToken.WaitHandle });
                    }

                    if (operation.Finished)
                    {
                        reparse = true;
                        operations.Remove(node);
                    }

                    node = next;
                }

                if (reparse)
                    filesToReparse.Add(file);
            }
            foreach (var file in filesToReparse)
            {
                ReparseMarkdownFile(file);
                if (PendingOperations[file].Count == 0)
                    PendingOperations.Remove(file);
            }
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
                    IndexedEntities.Remove(Path.ChangeExtension(path, ".html"));
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
            PendingOperations.Clear();
            IndexedEntities.Clear();
            IndexedMarkdownFiles.Clear();
            UnfinishedMarkdownFiles.Clear();
            ContextReferenceableEntities.Clear();
            Unlock();
        }

        public ValidationReport Validate(bool getFullReport, CancellationToken cancellationToken = default)
        {
            Lock();

            do
            {
                TryProcessPendingOperations(getFullReport, cancellationToken);
            }
            while (PendingOperations.Count != 0 && !cancellationToken.IsCancellationRequested);

            Debug.Assert(!getFullReport || PendingOperations.Count == 0 || cancellationToken.IsCancellationRequested);

            ValidationReport report = new ValidationReport(Configuration, PendingOperations.Count == 0);

            // Refresh references
            List<MarkdownFile> finishedFiles = new List<MarkdownFile>();
            foreach (var markdownFile in UnfinishedMarkdownFiles)
            {
                if (RefreshInternalContext(markdownFile, report))
                {
                    if (markdownFile.ParsingResult.ParserWarnings.Count == 0)
                        finishedFiles.Add(markdownFile);
                }
            }
            foreach (var finishedFile in finishedFiles)
            {
                finishedFile.ParsingResult.Release();
                UnfinishedMarkdownFiles.Remove(finishedFile);
            }

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
