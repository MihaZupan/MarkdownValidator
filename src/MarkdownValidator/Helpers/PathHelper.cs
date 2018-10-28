/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MihaZupan.MarkdownValidator.Helpers
{
    /// <summary>
    /// This class doesn't deal with all possible paths correctly!
    /// <para>It merely satisfies the internal needs of this library and should not be used elsewhere</para>
    /// </summary>
    public class PathHelper
    {
        private static readonly bool IsWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        private static readonly char Separator = IsWindows ? '\\' : '/';

        private readonly string RootDirectory;
        private readonly int RootDirectoryLength;

        internal PathHelper(string rootDirectory)
        {
            RootDirectory = rootDirectory;
            RootDirectoryLength = RootDirectory.Length;
        }

        internal bool TryGetContextRelativePath(string fullPath, out string relativePath)
        {
            if (fullPath.Length > RootDirectoryLength)
            {
                if (fullPath.StartsWith(RootDirectory, StringComparison.OrdinalIgnoreCase))
                {
                    relativePath = fullPath.Substring(RootDirectoryLength);
                    relativePath = relativePath.TrimEnd('/', '\\');
                    return true;
                }
            }
            else if (fullPath.Equals(RootDirectory, StringComparison.OrdinalIgnoreCase))
            {
                relativePath = string.Empty;
                return true;
            }

            relativePath = null;
            return false;
        }

        internal PathProcessingResult ProcessRelativePath(string sourceFile, string reference, out string relativePath)
        {
            relativePath = null;

            if (reference.OrdinalContains("://"))
                return PathProcessingResult.IsUrl;

            if (IsFsSpecific(reference))
                return PathProcessingResult.IsFsSpecific;

            string relative;

            if (reference.StartsWith('#'))
                relative = sourceFile + reference;
            else if (reference.OrdinalEquals("."))
                relative = Path.GetDirectoryName(sourceFile);
            else
                relative = Path.Combine(Path.GetDirectoryName(sourceFile), reference);

            return TryGetContextRelativePath(Path.GetFullPath(relative, RootDirectory), out relativePath)
                ? PathProcessingResult.OK
                : PathProcessingResult.NotInContext;
        }

        internal void EnsurePathIsValidForContext(string path, out string fullPath, out string relativePath)
        {
            fullPath = Path.GetFullPath(path, RootDirectory);
            if (!TryGetContextRelativePath(fullPath, out relativePath))
                throw new ArgumentException($"{path} is not a child path of the root working directory of the context");
        }

        internal enum PathProcessingResult
        {
            OK,
            NotInContext,
            IsUrl,
            IsFsSpecific,
        }

        public static string GetDirectoryWithSeparator(string path)
        {
            return path.Length == 0
                ? Environment.CurrentDirectory + Separator
                : Path.GetFullPath(path).TrimEnd('\\', '/') + Separator;
        }

        public static bool IsFsSpecific(string path)
        {
            if (path.Length != 0 && (path[0] == '/' || path[0] == '~'))
                return true;

            if (path.Length > 1 && path[1] == ':')
                return true;

            return false;
        }

        public static string GetPathFromFileUri(Uri fileUri)
        {
            return IsWindows
                ? fileUri.LocalPath.TrimStart('/')
                : fileUri.LocalPath;
        }
    }
}
