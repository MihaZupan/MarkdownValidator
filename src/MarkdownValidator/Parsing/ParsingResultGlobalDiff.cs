/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class ParsingResultGlobalDiff
    {
        public readonly List<ReferenceDefinition> RemovedReferenceDefinitions;
        public readonly List<ReferenceDefinition> NewReferenceDefinitions;

        public readonly List<string> RemovedReferences;

        public ParsingResultGlobalDiff(ParsingResult previous, ParsingResult current)
        {
            (RemovedReferenceDefinitions, NewReferenceDefinitions) =
                DiffHelper.Diff(previous.GlobalReferenceDefinitions, current.GlobalReferenceDefinitions);

            List<string> newReferences;
            (RemovedReferences, newReferences) =
                DiffHelper.Diff(previous.References.Keys, current.References.Keys);

            //current.UnprocessedReferences = new LinkedList<string>(newReferences);
        }
    }
}
