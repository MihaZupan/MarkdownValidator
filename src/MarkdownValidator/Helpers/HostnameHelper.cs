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
        public static bool IsDocumentationHostname(Uri url)
        {
            string host = url.Host;
            string dnsHost = url.DnsSafeHost;

            if (url.HostNameType == UriHostNameType.Dns)
            {
                if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                    return true;

                if (host.ContainsAny(StringComparison.OrdinalIgnoreCase,
                    "example.com",  // RFC2606
                    "example.org",  // RFC2606
                    "example.net")) // RFC2606
                    return true;

                if (host.EndsWith(".example", StringComparison.OrdinalIgnoreCase)) // RFC2606
                    return true;
            }
            else if (url.HostNameType == UriHostNameType.IPv4)
            {
                if (host.OrdinalEquals("127.0.0.1"))
                    return true;

                if (dnsHost.OrdinalEquals("0.0.0.0"))
                    return true;

                if (host.StartsWithAny(StringComparison.Ordinal,
                    "192.168.",     // RFC1918
                    "10.",          // RFC1918
                    "192.0.2.",     // RFC5737
                    "198.51.100.",  // RFC5737
                    "203.0.113."))  // RFC5737
                    return true;

                if (host.OrdinalStartsWith("172.") && // RFC1918
                    int.TryParse(host.Split('.')[1], out int secondPart) &&
                    secondPart >= 16 && secondPart <= 32)
                    return true;
            }
            else if (url.HostNameType == UriHostNameType.IPv6)
            {
                if (dnsHost.OrdinalEquals("::"))
                    return true;

                if (host.ContainsAny(StringComparison.OrdinalIgnoreCase,
                    "2001:DB8::"))  // RFC3849
                    return true;
            }

            return false;
        }
    }
}
