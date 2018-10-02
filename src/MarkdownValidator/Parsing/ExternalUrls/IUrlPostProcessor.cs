/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls
{
    public interface IUrlPostProcessor
    {
        string Identifier { get; }
        string[] Hostnames { get; }
        void Initialize(Config configuration);
        void Process(UrlPostProcessorContext context);
    }
}
