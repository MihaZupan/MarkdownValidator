using System;

namespace ReferenceValidator.Parser
{
    class MarkdownEntity
    {
        public MarkdownFile File;
        public readonly MarkdownEntityType Type;
        public readonly LineSpan Span;
        public readonly string Value;
        public readonly string CleanValue;

        public int StartLine => Span.Start;
        public bool IsOnOneLine => Span.IsOneLine;

        public MarkdownEntity(MarkdownEntityType type, LineSpan span, string value)
        {
            Type = type;
            Span = span;
            Value = value;

            CleanValue = value.Trim();

            if (type == MarkdownEntityType.ExternalLink)
            {
                if (CleanValue.Contains('#'))
                    CleanValue = CleanValue.SubstringBefore('#');
                CleanValue = CleanValue.Replace('\\', '/');
                if (CleanValue.EndsWith('>'))
                    CleanValue = CleanValue.TrimEnd('>');
                return;
            }

            CleanValue = value.ToLower();

            if (type == MarkdownEntityType.Header)
                CleanValue = CleanValue
                    .RemoveDouble(' ')
                    .Replace("`", "", StringComparison.Ordinal)
                    .Replace(' ', '-');
        }
    }
}
