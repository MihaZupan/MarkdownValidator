/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using MihaZupan.MarkdownValidator.WebIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace MihaZupan.MarkdownValidator.Parsing.ExternalUrls
{
    internal sealed class UrlProcessor
    {
        private readonly WebIOConfig WebConfig;
        private readonly Dictionary<string, List<IUrlPostProcessor>> UrlPostProcessors;
        internal void AddUrlPostProcessor(IUrlPostProcessor processor)
        {
            foreach (var hostname in processor.Hostnames)
            {
                if (UrlPostProcessors.TryGetValue(hostname, out var processors))
                {
                    processors.Add(processor);
                }
                else
                {
                    UrlPostProcessors.Add(hostname,
                        new List<IUrlPostProcessor>(4)
                        {
                            processor
                        });
                }
            }

            processor.Initialize(WebConfig.Configuration);
        }

        public UrlProcessor(WebIOConfig configuration)
        {
            WebConfig = configuration;
            UrlPostProcessors = new Dictionary<string, List<IUrlPostProcessor>>(StringComparer.OrdinalIgnoreCase);
        }

        public void ProcessUrl(ParsingContext context, Reference reference)
        {
            context.SetWarningSource(WarningSource.UrlProcessor);

            string urlString = reference.RawReference;

            if (urlString.StartsWith(':') ||
                urlString.EndsWith(':') ||
                !Uri.TryCreate(urlString, UriKind.RelativeOrAbsolute, out Uri url))
            {
                context.ReportWarning(
                    WarningIDs.InvalidUrlFormat,
                    reference,
                    "`{0}` is not a valid url",
                    urlString);
                return;
            }

            if (url.IsAbsoluteUri && HostnameHelper.IsDocumentationHostname(url))
                return;

            if (!Uri.TryCreate(urlString, UriKind.Absolute, out url))
            {
                context.ReportWarning(
                    WarningIDs.InvalidUrlFormat,
                    reference,
                    "`{0}` is not a valid url",
                    urlString);
                return;
            }

            Debug.Assert(url.IsAbsoluteUri);

            if (url.AbsoluteUri.OrdinalContains("..."))
                return;

            if (urlString.Contains('<', out int offset) && urlString.Contains('>', offset))
                return;

            if (url.HostNameType == UriHostNameType.IPv4 ||
                url.HostNameType == UriHostNameType.IPv6)
            {
                context.ReportWarning(
                    WarningIDs.UrlHostnameIsIP,
                    reference,
                    "Use a hostname instead of an IP address");
            }

            // ToDo: Check for local addresses

            // ToDo: Extensible scheme parsing support
            if (url.Scheme.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                ProcessHttpUrl(context, reference, url);
            }
        }

        private void ProcessHttpUrl(ParsingContext context, Reference reference, Uri originalUrl)
        {
            var cacheState = context.WebIO.TryGetSiteInfo(originalUrl, out SiteInfo info);
            if (cacheState == SiteCacheState.WebIODisabled)
                return;

            Uri url = info.Url.Url;
            string cleanUrl = info.Url.AbsoluteUrlWithoutFragment;

            if (cacheState == SiteCacheState.Unknown)
            {
                context.RegisterPendingOperation(context.WebIO.RequestSiteInfo(url));
                return;
            }

            if (cacheState == SiteCacheState.UnresolvableHostname)
            {
                Report_UnresolvableHostname(context, reference, url.Host);
                return;
            }

            Debug.Assert(cacheState == SiteCacheState.Cached);

            if (info.RequestFailed)
            {
                Report_WebRequestFailed(context, reference, cleanUrl);
                return;
            }

            if (info.RequestTimedOut)
            {
                Report_WebRequestTimedOut(context, reference, cleanUrl);
                return;
            }

            if (info.IsRedirect)
            {
                int maximumRedirects = Math.Max(0, context.Configuration.WebIO.MaximumRedirectCount) + 1;
                List<SiteInfo> redirectChain = new List<SiteInfo>(maximumRedirects) { info };

                while (redirectChain.Count < maximumRedirects)
                {
                    var nextUrl = redirectChain.Last().RedirectTarget;
                    cacheState = context.WebIO.TryGetSiteInfo(nextUrl.Url, out SiteInfo nextInfo);

                    if (cacheState == SiteCacheState.UnresolvableHostname)
                    {
                        Report_UnresolvableHostname(context, reference, nextUrl.Url.Host, cleanUrl);
                        return;
                    }

                    if (cacheState == SiteCacheState.Unknown)
                    {
                        context.RegisterPendingOperation(context.WebIO.RequestSiteInfo(nextUrl.Url));
                        return;
                    }

                    Debug.Assert(cacheState == SiteCacheState.Cached);

                    if (nextInfo.RequestFailed)
                    {
                        Report_WebRequestFailed(context, reference, nextUrl.AbsoluteUrlWithoutFragment, cleanUrl);
                        return;
                    }

                    if (nextInfo.RequestTimedOut)
                    {
                        Report_WebRequestTimedOut(context, reference, nextInfo.Url.AbsoluteUrlWithoutFragment, cleanUrl);
                        return;
                    }

                    if (nextInfo.IsRedirect)
                    {
                        redirectChain.Add(nextInfo);
                        continue;
                    }

                    if (!nextInfo.Is2xxStatusCode)
                    {
                        Report_WebRequestReturnedErrorCode(context, reference, nextUrl.AbsoluteUrlWithoutFragment, info.StatusCode, cleanUrl);
                        return;
                    }
                    else break;
                }

                if (redirectChain.Count >= maximumRedirects)
                {
                    context.ReportWarning(
                        WarningIDs.TooManyRedirects,
                        reference,
                        "`{0}` redirected more times than is allowed by the config ({1})",
                        cleanUrl,
                        maximumRedirects.ToString());
                    return;
                }

                string message = "`{0}` redirected to `{1}`";
                if (redirectChain.Count > 1)
                {
                    message += string.Format(" and {0} more location{1}",
                        redirectChain.Count - 1,
                        redirectChain.Count == 2 ? string.Empty : "s");
                }
                context.ReportWarning(
                    WarningIDs.RedirectChain,
                    reference,
                    message,
                    cleanUrl,
                    info.RedirectTarget.AbsoluteUrlWithoutFragment);
                return;
            }

            if (!info.Is2xxStatusCode)
            {
                Report_WebRequestReturnedErrorCode(context, reference, cleanUrl, info.StatusCode);
                return;
            }


            if (UrlPostProcessors.TryGetValue(url.Host, out var postProcessors) && postProcessors.Count != 0)
            {
                UrlPostProcessorContext postProcessorContext = new UrlPostProcessorContext(context, reference, info);
                foreach (var postProcessor in postProcessors)
                {
                    context.SetWarningSource(WarningSource.UrlPostProcessor, postProcessor.Identifier);
                    postProcessor.Process(postProcessorContext);
                }
            }
        }

        private void Report_WebRequestReturnedErrorCode(ParsingContext context, Reference reference, string url, HttpStatusCode code, string redirectedFrom = null)
        {
            if (redirectedFrom == null)
            {
                context.ReportWarning(
                    WarningIDs.WebRequestReturnedErrorCode,
                    reference,
                    "Request to `{0}` returned a non-successful status code ({1})",
                    url,
                    ((int)code).ToString());
            }
            else
            {
                context.ReportWarning(
                    WarningIDs.WebRequestReturnedErrorCode,
                    reference,
                    "Request to `{0}` (redirect from `{1}`) returned a non-successful status code ({1})",
                    url,
                    redirectedFrom,
                    ((int)code).ToString());
            }
        }
        private void Report_WebRequestTimedOut(ParsingContext context, Reference reference, string url, string redirectedFrom = null)
        {
            if (redirectedFrom == null)
            {
                context.ReportWarning(
                    WarningIDs.WebRequestTimedOut,
                    reference,
                    "Web request to `{0}` timed out ({1} ms)",
                    url,
                    context.Configuration.WebIO.RequestTimeout.ToString());
            }
            else
            {
                context.ReportWarning(
                    WarningIDs.WebRequestTimedOut,
                    reference,
                    "Web request to `{0}` (redirect from `{1}`) timed out ({2} ms)",
                    url,
                    redirectedFrom,
                    context.Configuration.WebIO.RequestTimeout.ToString());
            }
        }
        private void Report_UnresolvableHostname(ParsingContext context, Reference reference, string host, string redirectedFrom = null)
        {
            if (redirectedFrom == null)
            {
                context.ReportWarning(
                    WarningIDs.UnresolvableHostname,
                    reference,
                    "Could not resolve the hostname `{0}`",
                    host);
            }
            else
            {
                context.ReportWarning(
                    WarningIDs.UnresolvableHostname,
                    reference,
                    "`{0}` redirected to a hostname (`{1}`) that could not be resolved",
                    redirectedFrom,
                    host);
            }
        }
        private void Report_WebRequestFailed(ParsingContext context, Reference reference, string url, string redirectedFrom = null)
        {
            if (redirectedFrom == null)
            {
                context.ReportWarning(
                    WarningIDs.WebRequestFailed,
                    reference,
                    "Web request to `{0}` failed",
                    url);

            }
            else
            {
                context.ReportWarning(
                    WarningIDs.WebRequestFailed,
                    reference,
                    "Web request to `{0}` (redirect from `{1}`) failed",
                    url,
                    redirectedFrom);
            }
        }
    }
}
