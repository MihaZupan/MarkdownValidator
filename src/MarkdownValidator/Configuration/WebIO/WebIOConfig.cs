/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Parsing.ExternalUrls;
using MihaZupan.MarkdownValidator.WebIO;
using Newtonsoft.Json;
using System;
using System.Net;

namespace MihaZupan.MarkdownValidator.Configuration
{
    public class WebIOConfig
    {
        public bool Enabled = true;

        /// <summary>
        /// Defaults to 16
        /// </summary>
        public int MaximumRequestConcurrency
        {
            get => _maximumRequestConcurrency;
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaximumRequestConcurrency), value, "Must be a positive integer");
                _maximumRequestConcurrency = Math.Min(1024, value);
            }
        }
        private int _maximumRequestConcurrency = 16;

        /// <summary>
        /// Setting this to a non-positive value will make the parser stop trying after the first redirect
        /// <para>Defaults to 4</para>
        /// </summary>
        public int MaximumRedirectCount = 4;

        /// <summary>
        /// Defaults to 10000
        /// </summary>
        public int RequestTimeout
        {
            get => _requestTimeout;
            set
            {
                _requestTimeout = Math.Max(-1, value);
            }
        }
        private int _requestTimeout = 10000;

        [JsonIgnore]
        public IWebProxy Proxy = null;

        internal Config Configuration;
        internal void Initialize(Config configuration)
        {
            Configuration = configuration;
            WebIOController = new WebIOController(this);
            UrlProcessor = new UrlProcessor(this);
        }
        internal WebIOController WebIOController;
        internal UrlProcessor UrlProcessor;
    }
}
