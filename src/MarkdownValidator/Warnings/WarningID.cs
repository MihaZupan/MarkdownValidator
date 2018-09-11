/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
namespace MihaZupan.MarkdownValidator.Warnings
{
    /// <summary>
    /// All <see cref="WarningID"/>s with values higher than <see cref="Error"/> (1M) are considered errors
    /// </summary>
    public enum WarningID
    {
        Warning = 0,

        HugeFile,
        UnusedDefinedReference,
        EmptyCodeBlock,

        //
        // Errors
        //

        Error = 1000000,

        UnresolvedReference,
        DuplicateReferenceDefinition,
    }
}
