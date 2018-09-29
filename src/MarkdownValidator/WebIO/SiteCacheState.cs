/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/

namespace MihaZupan.MarkdownValidator.WebIO
{
    public enum SiteCacheState
    {
        /// <summary>
        /// Cached, but might not be successful
        /// </summary>
        Cached,
        /// <summary>
        /// Not yet requested
        /// </summary>
        Unknown,
        /// <summary>
        /// DNS resolve failed
        /// </summary>
        UnresolvableHostname,
        WebIODisabled
    }
}
