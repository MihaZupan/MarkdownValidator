/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MihaZupan.MarkdownValidator.WebIO
{
    public class SiteInfo
    {
        public readonly CleanUrl Url;
        internal SiteInfo(CleanUrl url)
        {
            Url = url;
        }

        public bool RequestTimedOut { get; internal set; } = false;
        public bool RequestFailed { get; internal set; } = false;
        public HttpRequestException HttpException { get; internal set; }

        public bool Is2xxStatusCode { get; internal set; }
        public HttpStatusCode StatusCode { get; internal set; }
        public Version HttpVersion { get; internal set; }

        public bool IsRedirect { get; internal set; }
        public CleanUrl RedirectTarget { get; internal set; }

        public HttpResponseHeaders Headers { get; internal set; }
        public HttpContentHeaders ContentHeaders { get; internal set; }
        public string ContentType => ContentHeaders.ContentType?.MediaType;
        public long? ContentLength => ContentHeaders.ContentLength;

        public bool ContentPresent { get; internal set; } = false;
        public bool ContentIsText { get; internal set; }
        public string ContentText { get; internal set; }
        public byte[] ContentBytes { get; internal set; }
    }
}
