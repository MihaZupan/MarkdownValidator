/*
    Copyright (c) Miha Zupan. All rights reserved.
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
        public readonly WarningSource Source;
        public readonly string ParserIdentifier;
        public readonly string Value;
        public readonly string Message;
        public readonly bool IsError;
        public readonly bool IsSuggestion;

        internal Warning(WarningID id, WarningLocation location, string value, string message, WarningSource source, string parserIdentifier)
        {
            Location = location;
            ID = id;
            Source = source;
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
                other.Source.Equals(Source) &&
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
                hash ^= (int)Source;
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
                    compare = Message.CompareTo(other.Message);

                    if (compare == 0)
                    {
                        // Sort by Value
                        compare = Value.CompareTo(other.Value);

                        if (compare == 0)
                        {
                            // Sort by source
                            compare = Source.CompareTo(other.Source);
                        }
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
