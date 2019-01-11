/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Helpers;
using System;
using System.Diagnostics;

namespace MihaZupan.MarkdownValidator.Warnings
{
    [DebuggerDisplay("{ID.Identifier}, {Location.StartLine}-{Location.EndLine}, {Location.Span.Start}-{Location.Span.End}, {Value}")]
    public sealed class Warning : IEquatable<Warning>, IComparable<Warning>
    {
        public readonly WarningLocation Location;
        public readonly WarningID ID;
        public readonly string ParserIdentifier;
        public readonly string Value;
        public readonly string Message;
        public readonly bool IsError;
        public readonly bool IsSuggestion;

        internal Warning(WarningID id, WarningLocation location, string value, string message, string parserIdentifier)
        {
            Location = location;
            ID = id;
            Value = value;
            Message = message;
            IsError = id.IsError;
            IsSuggestion = id.IsSuggestion;
            ParserIdentifier = parserIdentifier;
        }

        public bool Equals(Warning other)
        {
            if (other is null) return false;
            return
                other.ID.Equals(ID) &&
                other.Location.Equals(Location) &&
                other.Value.OrdinalEquals(Value) &&
                other.Message.OrdinalEquals(Message);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Location.GetHashCode();
                hash = (hash * 397) ^ Message.Length;
                hash ^= ID.GetHashCode();
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is Warning other)
            {
                return other.Equals(this);
            }
            return false;
        }

        public int CompareTo(Warning other)
        {
            // Sort by location
            int compare = Location.CompareTo(other.Location);

            if (compare == 0)
            {
                // Sort by ID
                compare = ID.CompareTo(other.ID);

                if (compare == 0)
                {
                    // Sort by message
                    compare = StringComparer.Ordinal.Compare(Message, other.Message);

                    if (compare == 0)
                    {
                        // Sort by Value
                        compare = StringComparer.Ordinal.Compare(Value, other.Value);
                    }
                }
            }

            return compare;
        }

        public static bool operator ==(Warning a, Warning b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(Warning a, Warning b)
            => !(a == b);

        public override string ToString()
        {
            return string.Format("{0}, {1}-{2}, {3}-{4}, {5}",
                ID.Identifier,
                Location.StartLine,
                Location.EndLine,
                Location.Span.Start,
                Location.Span.End,
                Message);
        }
    }
}
