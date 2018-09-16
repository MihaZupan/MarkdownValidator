/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Diagnostics;

namespace MihaZupan.MarkdownValidator
{
    [DebuggerDisplay("{Value}")]
    internal sealed class ReferenceInt : IEquatable<ReferenceInt>, IComparable<ReferenceInt>
    {
        public int Value;

        public ReferenceInt(int value = 0)
        {
            Value = value;
        }

        public int CompareTo(ReferenceInt other)
            => Value.CompareTo(other.Value);

        public bool Equals(ReferenceInt other)
        {
            if (other is null)
                return false;

            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (obj is ReferenceInt other)
                return Equals(other);
            return false;
        }

        public override int GetHashCode()
            => Value.GetHashCode();

        public static bool operator ==(ReferenceInt a, ReferenceInt b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(ReferenceInt a, ReferenceInt b)
            => !(a == b);

        public static ReferenceInt operator ++(ReferenceInt value)
            => new ReferenceInt(value.Value++);
        public static ReferenceInt operator --(ReferenceInt value)
            => new ReferenceInt(value.Value--);

        public static implicit operator int(ReferenceInt value)
            => value.Value;
    }
}
