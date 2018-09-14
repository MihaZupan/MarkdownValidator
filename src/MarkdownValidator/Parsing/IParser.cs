﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
namespace MihaZupan.MarkdownValidator.Parsing
{
    public interface IParser
    {
        void Initialize(ParserRegistrationContext context);
    }
}