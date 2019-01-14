/*
    Copyright (c) 2019 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.WebIO;
using System;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls.UrlPostProcessors.GitHub
{
    internal static class GitHubRawUserContent
    {
        public static bool TryGetRawUserContentUrl(CleanUrl url, out Uri rawContentUrl)
        {
            rawContentUrl = null;

            string path = url.Url.AbsolutePath;
            if (path.Length < 11)
                return false;

            int repositoryNameIndex = path.IndexOf('/', 1);
            if (repositoryNameIndex == -1)
                return false;

            int linkTypeIndex = path.IndexOf('/', repositoryNameIndex + 1);
            if (linkTypeIndex == -1)
                return false;

            if (path.Length < linkTypeIndex - 8)
                return false;
            linkTypeIndex++;

            if (!path.OrdinalEqualsAtIndex("blob", linkTypeIndex))
                return false;

            rawContentUrl = new Uri(string.Concat("https://raw.githubusercontent.com", path.Substring(0, linkTypeIndex), path.Substring(linkTypeIndex + 5)));
            return true;
        }
    }
}
