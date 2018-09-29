/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System.Collections.Generic;
using System.Threading;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class PendingOperation
    {
        internal MarkdownFile File;
        internal ManualResetEvent MRE;

        public bool Finished { get; private set; } = false;

        private LinkedList<PendingOperation> AttachedOperations;

        private PendingOperation(bool finished)
        {
            Finished = finished;
        }
        internal static PendingOperation New()
        {
            return new PendingOperation(false)
            {
                MRE = new ManualResetEvent(false)
            };
        }
        internal static PendingOperation Completed()
            => new PendingOperation(true);
        internal PendingOperation Attach()
        {
            lock (this)
            {
                if (Finished)
                    return Completed();

                var operation = new PendingOperation(false)
                {
                    MRE = this.MRE
                };

                if (AttachedOperations is null)
                    AttachedOperations = new LinkedList<PendingOperation>();

                AttachedOperations.AddLast(operation);

                return operation;
            }
        }

        internal void SignalCompletedInternal()
        {
            Finished = true;
            lock (this)
            {
                if (AttachedOperations != null)
                {
                    foreach (var operation in AttachedOperations)
                    {
                        operation.SignalCompletedInternal();
                    }
                }
            }
        }
        public void SignalCompleted()
        {
            if (Finished) return;

            lock (this)
            {
                Finished = true;
                MRE.Set();
            }
            SignalCompletedInternal();
        }
    }
}
