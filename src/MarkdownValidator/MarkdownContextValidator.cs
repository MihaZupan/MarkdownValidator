/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using System;
using System.Threading;

namespace MihaZupan.MarkdownValidator
{
    /// <summary>
    /// Represents a context of markdown files and other referenceable files and directories.
    /// <para>It can be updated in real time, allowing for use in IDEs</para>
    /// <para>All method calls on an instance of this class are thread-safe</para>
    /// <para>It is up to you to make sure you call all adds/updates before calling Validate</para>
    /// </summary>
    public class MarkdownContextValidator
    {
        public readonly Config Configuration;
        private readonly ValidationContext Context;

        /// <summary>
        /// Constructs a new validation context with the specified <see cref="Config"/>
        /// </summary>
        /// <param name="configuration">The configuration to use when constructing, parsing and validating the context</param>
        public MarkdownContextValidator(Config configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Context = new ValidationContext(Configuration);
        }

        public bool UpdateMarkdownFile(string filePath, string markdownSource)
        {
            EnsureValidPath(filePath, out string relativePath);
            return Context.UpdateMarkdownFile(relativePath, markdownSource);
        }

        public bool AddMarkdownFile(string filePath, string markdownSource)
        {
            EnsureValidPath(filePath, out string fullPath, out string relativePath);
            return Context.AddMarkdownFile(fullPath, relativePath, markdownSource);
        }

        public bool AddEntity(string path)
        {
            EnsureValidPath(path, out string relativePath);
            return Context.IndexEntity(relativePath);
        }
        public bool RemoveEntity(string path)
        {
            EnsureValidPath(path, out string relativePath);
            return Context.RemoveEntityFromIndex(relativePath);
        }

        /// <summary>
        /// This method will return very quickly, but does not guarantee a complete report if any async operations are still in progress internally
        /// <para>Setting <paramref name="fully"/> to true is the same as calling <see cref="ValidateFully"/></para>
        /// </summary>
        /// <param name="fully">Setting this to true is the same as calling <see cref="ValidateFully"/></param>
        /// <returns></returns>
        public ValidationReport Validate(bool fully = false)
            => Context.Validate(fully);
        /// <summary>
        /// This method may block if any async operations are still in progress internally
        /// <para>If the <paramref name="cancellationToken"/> isn't signaled,
        /// the <see cref="ValidationReport"/> is guaranteed to be complete</para>
        /// </summary>
        /// <returns></returns>
        public ValidationReport ValidateFully(CancellationToken cancellationToken = default)
            => Context.Validate(true, cancellationToken);

        public void Clear() => Context.Clear();

        private void EnsureValidPath(string path, out string relativePath)
            => EnsureValidPath(path, out _, out relativePath);
        private void EnsureValidPath(string path, out string fullPath, out string relativePath)
            => Configuration.EnsurePathIsValidForContext(path, out fullPath, out relativePath);
    }
}
