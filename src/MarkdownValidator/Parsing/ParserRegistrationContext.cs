﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using System;

namespace MihaZupan.MarkdownValidator.Parsing
{
    public sealed class ParserRegistrationContext
    {
        private readonly ParsingController ParsingController;
        public readonly Config Configuration;

        internal ParserRegistrationContext(ParsingController parsingController, Config configuration)
        {
            ParsingController = parsingController;
            Configuration = configuration;
        }

        public void Register(Type type, Action<ParsingContext> action)
            => ParsingController.Register(type, action);
    }
}
