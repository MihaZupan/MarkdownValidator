/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;

namespace MihaZupan.MarkdownValidator.WebIO
{
    public class CleanUrl
    {
        public readonly Uri Url;
        public readonly string AbsoluteUrlWithoutFragment;

        public CleanUrl(Uri url)
        {
            Url = url;
            if (url.Fragment.Length != 0)
            {
                AbsoluteUrlWithoutFragment = url.AbsoluteUri.Substring(0, url.AbsoluteUri.Length - url.Fragment.Length);
            }
            else AbsoluteUrlWithoutFragment = url.AbsoluteUri;
        }
    }
}
