/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.Helpers
{
    internal static class HostnameHelper
    {
        public static bool IsDocumentationHostname(string hostname)
        {
            if (hostname.ContainsAny(StringComparison.OrdinalIgnoreCase,
                "example.com",  // RFC2606
                "example.org",  // RFC2606
                "example.net",  // RFC2606
                "2001:DB8::",   // RFC3849
                "..."))         // http://...
                return true;

            if (hostname.EndsWith(".example", StringComparison.OrdinalIgnoreCase)) // RFC2606
                return true;

            if (hostname.StartsWithAny(StringComparison.Ordinal,
                "192.0.2.",     // RFC5737
                "198.51.100.",  // RFC5737
                "203.0.113."))  // RFC5737
                return true;

            return false;
        }
    }
}
