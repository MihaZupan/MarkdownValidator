/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing.ExternalUrls;

namespace MihaZupan.MarkdownValidator.ExternalParsers
{
    public class GithubPostProcessor : IUrlPostProcessor
    {
        public string Identifier => nameof(GithubPostProcessor);

        public string[] Hostnames => new[] { "github.com" };

        public void Initialize(Config configuration)
        {
            configuration.WebIO.RegisterDownloadableContent("raw.githubusercontent.com", "text/plain", true);
        }

        public void Process(UrlPostProcessorContext context)
        {

        }
    }
}
