/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public interface IPipelineProvider
    {
        /// <summary>
        /// Each call to this method MUST return a NEW instance of <see cref="MarkdownPipelineBuilder"/>
        /// <para>Calls to this method may happen concurrently from multiple threads</para>
        /// </summary>
        /// <returns></returns>
        MarkdownPipelineBuilder GetNewPipeline();
    }
}
