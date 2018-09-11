/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System.Threading;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public class AsyncProgress
    {
        internal readonly MarkdownFile File;
        internal readonly ManualResetEvent MRE = new ManualResetEvent(false);

        public bool Finished { get; private set; } = false;
        public readonly int SuggestedWait;

        internal AsyncProgress(MarkdownFile file, int suggestedWait = 100)
        {
            File = file;
            SuggestedWait = suggestedWait;
        }

        public void SignalCompleted()
        {
            Finished = true;
            MRE.Set();
        }
    }
}
