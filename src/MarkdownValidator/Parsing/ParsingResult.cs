﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Warnings;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class ParsingResult
    {
        public string Source;
        public MarkdownDocument SyntaxTree;

        internal int LastLinkDelimiterIndex = -1;
        internal int LastLinkInlineIndex = -1;

        public List<Warning> ParserWarnings { get; private set; } = new List<Warning>();
        public List<ReferenceDefinition> GlobalReferenceDefinitions = new List<ReferenceDefinition>();
        public List<ReferenceDefinition> LocalReferenceDefinitions = new List<ReferenceDefinition>();

        public readonly Dictionary<string, List<Reference>> References = new Dictionary<string, List<Reference>>(StringComparer.OrdinalIgnoreCase);
        public LinkedList<string> UnprocessedReferences { get; internal set; } = new LinkedList<string>();

        public readonly List<AsyncProgress> AsyncOperations = new List<AsyncProgress>();

        public ParsingResult(string source, MarkdownPipeline pipeline)
        {
            Source = source;
            SyntaxTree = Markdown.Parse(source, pipeline);
        }
        public ParsingResult(ParsingResult parsingResult)
        {
            Source = parsingResult.Source;
            SyntaxTree = parsingResult.SyntaxTree;
        }

        public void Finalize(ParsingContext context)
        {
            context.SetWarningSource(WarningSource.ParserFinalize);

            Dictionary<string, ReferenceDefinition> definedReferences = new Dictionary<string, ReferenceDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var definition in LocalReferenceDefinitions)
            {
                if (!definedReferences.TryAdd(definition.RawReference, definition))
                {
                    context.ReportWarning(
                        WarningID.DuplicateReferenceDefinition,
                        definition,
                        "Duplicate reference definition `{0}` - already defined on line {1}.",
                        definition.RawReference,
                        (definedReferences[definition.RawReference].Line + 1).ToString());
                }
            }

            foreach (var referenceList in References.Values)
            {
                var reference = referenceList.First();
                if (definedReferences.TryGetValue(
                    reference.RawReference,
                    out ReferenceDefinition first))
                {
                    first.Used = true;
                }
                else if (reference.RawReference.Contains(':'))
                {
                    // Hand over links to the ExternalLinkContext

                }
                else
                {
                    UnprocessedReferences.AddLast(reference.GlobalReference);
                }
            }

            foreach (var definition in definedReferences.Values)
            {
                if (!definition.Used)
                {
                    context.ReportWarning(
                        WarningID.UnusedDefinedReference,
                        definition,
                        "Unused defined reference `{0}`.",
                        definition.RawReference);
                }
            }

            // Release some resources
            Source = null;
            SyntaxTree = null;
            //LocalReferenceDefinitions = null;
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
    }
}
