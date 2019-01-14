/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Parsing.ExternalUrls;
using MihaZupan.MarkdownValidator.WebIO;
using System;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class ParserRegistrationContext
    {
        public readonly Config Configuration;
        public readonly WebIOConfig WebIOConfig;

        private readonly ParsingController ParsingController;
        internal string ParserIdentifier;

        internal ParserRegistrationContext(ParsingController parsingController, Config configuration)
        {
            ParsingController = parsingController;
            Configuration = configuration;
            WebIOConfig = Configuration.WebIO;
        }

        public TCustomConfig ReadConfig<TCustomConfig>(bool writeIfDefault = false) where TCustomConfig : class, new()
            => Configuration.ReadConfig<TCustomConfig>(ParserIdentifier, writeIfDefault);

        public void WriteConfig(object config)
            => Configuration.WriteConfig(ParserIdentifier, config);

        public void Register(Type type, Action<ParsingContext> action)
            => ParsingController.Register(type, ParserIdentifier, action);

        public void RegisterFinalizer(Action<ParsingContext> action)
            => ParsingController.RegisterFinalizer(ParserIdentifier, action);

        public void RegisterUrlRewriter(string hostname, Func<Uri, Uri> rewriter)
            => WebIOConfig.WebIOController.AddUrlRewriter(hostname, rewriter);

        public void RegisterDownloadableContent(string hostname, string contentType, bool isText, Func<CleanUrl, bool> urlSelector)
            => WebIOConfig.WebIOController.AddDownloadableContentType(hostname, contentType, isText, urlSelector);

        public void RegisterDownloadableContent(string hostname, string contentType, bool isText)
            => WebIOConfig.WebIOController.AddDownloadableContentType(hostname, contentType, isText, _ => true);

        public void RegisterUrlPostProcessor(string hostname, Action<UrlPostProcessorContext> action)
            => WebIOConfig.UrlProcessor.AddUrlPostProcessor(hostname, ParserIdentifier, action);
    }
}
