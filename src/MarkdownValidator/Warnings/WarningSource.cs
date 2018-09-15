﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Warnings
{
    [Flags]
    public enum WarningSource
    {
        InternalParser = 1 << 0,
        ExternalParser = 1 << 1,

        CodeBlockParser = 1 << 2,
        ParserFinalize = 1 << 3,

        LinkReferenceProcessor = InternalParser | 1 << 20,
        ParsingResultFinalize = InternalParser | 1 << 22,
        Validator = InternalParser | 1 << 23,

        InternalCodeBlockParser = InternalParser | CodeBlockParser,
        ExternalCodeBlockParser = ExternalParser | CodeBlockParser,

        InternalParserFinalize = InternalParser | ParserFinalize,
        ExternalParserFinalize = ExternalParser | ParserFinalize,
    }
}
