/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;

namespace MihaZupan.MarkdownValidator.WebIO
{
    public sealed class WebIOController
    {
        private readonly Config Configuration;

        internal WebIOController(Config configuration)
        {
            Configuration = configuration;
        }
    }
}
