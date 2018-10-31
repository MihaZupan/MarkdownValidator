/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Helpers;
using System;
using System.Collections.Generic;

namespace MihaZupan.MarkdownValidator.Parsing
{
    internal class ParsingResultGlobalDiff
    {
        public readonly List<ReferenceDefinition> RemovedHeadingDefinitions;
        public readonly List<ReferenceDefinition> NewHeadingDefinitions;

        public readonly List<string> RemovedReferences;

        public ParsingResultGlobalDiff(ParsingResult previous, ParsingResult current)
        {
            (RemovedHeadingDefinitions, NewHeadingDefinitions) =
                DiffHelper.Diff(previous.HeadingDefinitions, current.HeadingDefinitions);

            List<string> newReferences;
            (RemovedReferences, newReferences) =
                DiffHelper.Diff(previous.References.Keys, current.References.Keys, StringComparer.OrdinalIgnoreCase);
        }
    }
}
