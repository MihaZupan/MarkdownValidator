﻿/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class ParsingResult
    {
        public string Source;
        public MarkdownDocument SyntaxTree;

        public readonly List<int> LineStartIndexes;
        public Dictionary<Type, object> ParserStorage = new Dictionary<Type, object>();
        public readonly List<Warning> ParserWarnings = new List<Warning>();
        public readonly List<ReferenceDefinition> HeadingDefinitions = new List<ReferenceDefinition>();
        public List<ReferenceDefinition> LocalReferenceDefinitions = new List<ReferenceDefinition>();
        public readonly Dictionary<string, List<Reference>> References = new Dictionary<string, List<Reference>>(StringComparer.OrdinalIgnoreCase);
        public readonly LinkedList<string> UnprocessedReferences = new LinkedList<string>();
        public readonly LinkedList<PendingOperation> PendingOperations = new LinkedList<PendingOperation>();

        public ParsingResult(string source, Config configuration)
        {
            Debug.Assert(source != null);
            Source = source;
            SyntaxTree = Markdown.Parse(source, configuration.MarkdigPipeline);

            Debug.Assert(SyntaxTree.LineStartIndexes != null);
            LineStartIndexes = SyntaxTree.LineStartIndexes;
        }
        public ParsingResult(ParsingResult parsingResult)
        {
            Source = parsingResult.Source;
            SyntaxTree = parsingResult.SyntaxTree;
            LineStartIndexes = parsingResult.LineStartIndexes;
        }

        public void Finalize(ParsingContext context)
        {
            Dictionary<string, ReferenceDefinition> definedReferences
                = new Dictionary<string, ReferenceDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in LocalReferenceDefinitions)
            {
                if (!definedReferences.TryAdd(definition.RawReference, definition))
                {
                    context.ReportWarning(
                        WarningIDs.DuplicateReferenceDefinition,
                        definition,
                        "Duplicate reference definition `{0}` - already defined on line {1}",
                        definition.RawReference,
                        (definedReferences[definition.RawReference].Line + 1).ToString());
                }
            }
            Span<int> headingLines = stackalloc int[HeadingDefinitions.Count];
            for (int i = 0; i < HeadingDefinitions.Count; i++)
                headingLines[i] = HeadingDefinitions[i].Line;

            foreach (var referenceList in References.Values)
            {
                // Ignore references that are located on heading lines
                if (headingLines.Length > 0)
                {
                    for (int i = referenceList.Count - 1; i >= 0; i--)
                    {
                        if (headingLines.BinarySearch(referenceList[i].Line) >= 0)
                        {
                            referenceList.RemoveAt(i); // This shouldn't happen too often, List.RemoveAt is fine
                        }
                    }
                    if (referenceList.Count == 0)
                        continue;
                }

                var reference = referenceList.First();
                if (definedReferences.TryGetValue(
                    reference.RawReference,
                    out ReferenceDefinition first))
                {
                    first.Used = true;
                }
                else if (reference is LinkReference)
                {
                    foreach (LinkReference linkReference in referenceList)
                    {
                        context.ProcessLinkReference(linkReference);
                    }
                }
                else if (reference.GlobalReference.Length != 0)
                {
                    UnprocessedReferences.AddLast(reference.GlobalReference);
                }
            }

            foreach (var definition in definedReferences.Values)
            {
                if (!definition.Used)
                {
                    context.ReportWarning(
                        WarningIDs.UnusedDefinedReference,
                        definition,
                        "Unused defined reference `{0}`",
                        definition.RawReference);
                }
            }
        }
        public void AddReference(Reference referencedEntity)
        {
            if (References.TryGetValue(referencedEntity.GlobalReference, out List<Reference> references))
            {
                references.Add(referencedEntity);
            }
            else
            {
                References.Add(referencedEntity.GlobalReference, new List<Reference>() { referencedEntity });
            }
        }

        public void Release()
        {
            Source = null;
            SyntaxTree = null;
            LocalReferenceDefinitions = null;
            ParserStorage = null;
        }
    }
}
