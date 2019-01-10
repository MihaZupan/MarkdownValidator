/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System.Diagnostics;
using System.Threading;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class PendingOperation
    {
        private PendingOperation Parent;

        internal MarkdownFile File;

        private ManualResetEvent _mre = null;
        internal ManualResetEvent MRE
        {
            get
            {
                return Parent?._mre ?? _mre;
            }
        }

        private bool _finished = false;
        internal bool Finished
        {
            get
            {
                return Parent?._finished ?? _finished;
            }
        }

        private PendingOperation(bool finished)
        {
            if (finished)
            {
                _finished = true;
            }
            else
            {
                _mre = new ManualResetEvent(false);
            }
        }
        private PendingOperation(PendingOperation parentOperation)
        {
            if (parentOperation.Finished)
            {
                _finished = true;
            }
            else
            {
                Parent = parentOperation.Parent ?? parentOperation;
            }
        }

        internal static PendingOperation New()
            => new PendingOperation(false);
        internal static PendingOperation Completed()
            => new PendingOperation(true);
        internal PendingOperation Attach()
            => new PendingOperation(this);

        public void SignalCompleted()
        {
            Debug.Assert(Parent == null, "SignalCompleted should only be called on the parent PendingOperation");
            Debug.Assert(_finished == false, "SignalCompleted should only be called once");

            _finished = true;
            MRE.Set();
        }
    }
}
