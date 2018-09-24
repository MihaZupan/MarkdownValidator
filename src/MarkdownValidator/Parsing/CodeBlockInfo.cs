/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Helpers;
using Markdig.Syntax;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public class CodeBlockInfo
    {
        public readonly string Info;
        public readonly string Arguments;
        public readonly StringLineGroup Lines;
        public readonly StringSlice CodeBlockSlice;
        public readonly string Source;
        public readonly SourceSpan ContentSpan;
        public readonly StringSlice ContentSlice;
        public readonly string Content;

        internal CodeBlockInfo(FencedCodeBlock codeBlock, StringSlice slice)
        {
            Info = codeBlock.Info ?? string.Empty;
            Arguments = codeBlock.Arguments ?? string.Empty;
            Lines = codeBlock.Lines;
            CodeBlockSlice = slice;
            Source = CodeBlockSlice.Text;

            int contentStart = Lines.Lines[0].Position;
            var lastLine = Lines.Last();
            int contentEnd = lastLine.Position + lastLine.Slice.Length - 1;
            ContentSpan = new SourceSpan(contentStart, contentEnd);
            ContentSlice = CodeBlockSlice.Reposition(ContentSpan);
            Content = ContentSlice.ToString();
        }
    }
}
