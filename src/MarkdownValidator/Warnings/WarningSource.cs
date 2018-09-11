using System;

namespace MihaZupan.MarkdownValidator.Warnings
{
    [Flags]
    public enum WarningSource
    {
        InternalParser = 1,
        ExternalParser = 2,
        CodeBlockParser = 4,

        ParserFinalize = InternalParser | 8,
        Validator = InternalParser | 16,

        InternalCodeBlockParser = InternalParser | CodeBlockParser,
        ExternalCodeBlockParser = ExternalParser | CodeBlockParser,
    }
}
