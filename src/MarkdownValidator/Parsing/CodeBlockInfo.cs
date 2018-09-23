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
        public readonly StringSlice Slice;
        public readonly string Source;
        public readonly StringSlice ContentSlice;

        public string Content
        {
            get
            {
                if (_content is null)
                {
                    if (ContentSlice.Length <= 0)
                        _content = string.Empty;
                    else
                        _content = ContentSlice.ToString();
                }
                return _content;
            }
        }
        private string _content;

        internal CodeBlockInfo(FencedCodeBlock codeBlock, StringSlice slice)
        {
            Info = codeBlock.Info;
            Arguments = codeBlock.Arguments;
            Lines = codeBlock.Lines;            
            Slice = slice;
            Source = Slice.Text;

            int contentStart = Lines.Lines[0].Position;
            var lastLine = Lines.Last();
            int contentEnd = lastLine.Position + lastLine.Slice.Length;
            ContentSlice = Slice.Reposition(new SourceSpan(contentStart, contentEnd));
        }
    }
}
