/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Warnings;

namespace MihaZupan.MarkdownValidator.ExternalParsers
{
    public static class ExternalWarningIDs
    {
        //
        // Warnings
        //
        public const int WarningID = WarningIDs.ErrorID / 2;
        public static readonly WarningID InvalidListNumberOrder     = new WarningID(WarningID + 1,  nameof(InvalidListNumberOrder));
        public static readonly WarningID InvalidJsonInJsonCodeBlock = new WarningID(WarningID + 2,  nameof(InvalidJsonInJsonCodeBlock));

        //
        // Errors
        //
        public const int ErrorID = WarningIDs.ErrorID * 2;
    }
}
