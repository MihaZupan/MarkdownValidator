namespace ReferenceValidator.Parser
{
    class LineSpan
    {
        public readonly int Start;
        public readonly int End;

        public int Length => 1 + End - Start;
        public bool IsOneLine => Start == End;

        public LineSpan(int start, int end)
        {
            Start = start;
            End = end;
        }
    }
}
