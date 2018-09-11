/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Warnings
{
    public class Warning : IEquatable<Warning>
    {
        public readonly WarningLocation Location;
        public readonly WarningID ID;
        public readonly WarningSource Source;
        public readonly string Message;
        public readonly bool IsError;

        internal Warning(WarningID id, WarningLocation location, string message, WarningSource source)
        {
            Location = location;
            ID = id;
            Source = source;
            Message = message;
            IsError = id >= WarningID.Error;
        }

        public bool Equals(Warning other)
        {
            if (other is null) return false;
            return
                other.ID.Equals(ID) &&
                other.Location.Equals(Location) &&
                other.Source.Equals(Source) &&
                other.Message.OrdinalEquals(Message);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Location.GetHashCode();
                hash = (hash * 397) ^ Message.Length;
                hash ^= (int)ID;
                hash ^= (int)Source;
                return hash;
            }
        }
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (obj is Warning other)
            {
                return other.Equals(this);
            }
            return false;
        }
    }
}
