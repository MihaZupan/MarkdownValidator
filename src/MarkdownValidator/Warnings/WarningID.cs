/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Warnings
{
    /// <summary>
    /// All <see cref="WarningID"/>s with values higher than <see cref="WarningIDs.ErrorID"/> are considered errors
    /// <para>All <see cref="WarningID"/>s with negative values are considered suggestions</para>
    /// </summary>
    public class WarningID : IEquatable<WarningID>, IComparable<WarningID>
    {
        public readonly int ID;
        public readonly string Identifier;
        public readonly bool IsError;
        public readonly bool IsSuggestion;

        public WarningID(int id, string identifier)
        {
            ID = id;
            Identifier = identifier;
            IsError = id > WarningIDs.ErrorID;
            IsSuggestion = id < 0;
        }

        public override int GetHashCode()
            => ID.GetHashCode();
        public override bool Equals(object obj)
        {
            if (obj is WarningID other)
            {
                return Equals(other);
            }
            return false;
        }
        public bool Equals(WarningID other)
        {
            if (other is null)
                return false;
            return ID == other.ID;
        }

        public static bool operator ==(WarningID a, WarningID b)
        {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(WarningID a, WarningID b)
            => !(a == b);

        public int CompareTo(WarningID other)
            => ID.CompareTo(other.ID);
    }
}
