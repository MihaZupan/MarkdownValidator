/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Parsing;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MihaZupan.MarkdownValidator.WebIO
{
    public sealed class WebIOController
    {
        private readonly WebIOConfig WebConfig;
        private readonly bool Enabled;
        private readonly HttpClient HttpClient;
        internal WebIOController(WebIOConfig configuration)
        {
            WebConfig = configuration;
            Enabled = WebConfig.Enabled;
            MaxConcurrency = WebConfig.MaximumRequestConcurrency;

            if (MaxConcurrency <= 0)
                Enabled = false;

            if (!Enabled)
                return;

            var clientHandler = new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };
            if (WebConfig.Proxy != null)
            {
                clientHandler.Proxy = WebConfig.Proxy;
                clientHandler.UseProxy = true;
            }
            HttpClient = new HttpClient(clientHandler);
            HttpClient.DefaultRequestHeaders.Add(
                "User-Agent", "MihaZupan.MarkdownValidator-" + Config.Version.ToString());
            HttpClient.DefaultRequestHeaders.Add(
                "Accept", "*/*");
            HttpClient.DefaultRequestHeaders.Add(
                "Accept-Encoding", "gzip, deflate");
            HttpClient.DefaultRequestHeaders.Add(
                "Accept-Language", "en-GB,en;q=0.9");
            HttpClient.Timeout = TimeSpan.FromMilliseconds(WebConfig.RequestTimeout);

            DnsUnresolvableHostNames        = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            KnownGoodHostNames              = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            PendingRequestsByHostname       = new Dictionary<string, HostnameRequests>(StringComparer.OrdinalIgnoreCase);
            PendingRequests                 = new Dictionary<string, PendingRequest>(StringComparer.OrdinalIgnoreCase);
            RequestsInProgress              = new Dictionary<string, PendingRequest>(StringComparer.OrdinalIgnoreCase);
            CachedSites                     = new Dictionary<string, SiteInfo>(StringComparer.OrdinalIgnoreCase);
            DownloadableContentByHostname   = new Dictionary<string, List<(string, bool)>>();
            UrlRewriters                    = new Dictionary<string, Func<Uri, Uri>>(StringComparer.OrdinalIgnoreCase);

            SchedulerMRE = new ManualResetEvent(false);
            Task.Run(() => SchedulerWorkInternal());
        }

        private readonly Dictionary<string, List<(string, bool)>> DownloadableContentByHostname;
        internal void AddDownloadableContentType(string hostname, string contentType, bool isText)
        {
            if (DownloadableContentByHostname.TryGetValue(hostname, out var downloadableContent))
            {
                for (int i = 0; i < downloadableContent.Count; i++)
                {
                    var content = downloadableContent[i];
                    if (content.Item1.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                    {
                        if (isText && !content.Item2)
                            downloadableContent[i] = (contentType, true);
                        return;
                    }
                }
                downloadableContent.Add((contentType, isText));
            }
            else
            {
                DownloadableContentByHostname.Add(hostname,
                    new List<(string, bool)>(4)
                    {
                        (contentType, isText)
                    });
            }
        }
        private bool TryGetDownloadableContentType(string hostname, string contentType, out bool isText)
        {
            if (DownloadableContentByHostname.TryGetValue(hostname, out var downloadableContent))
            {
                if (downloadableContent.ContainsAny(c => c.Item1.Equals(contentType, StringComparison.OrdinalIgnoreCase), out var content))
                {
                    isText = content.Item2;
                    return true;
                }
            }

            isText = false;
            return false;
        }

        private readonly Dictionary<string, Func<Uri, Uri>> UrlRewriters;
        internal void AddUrlRewriter(string hostname, Func<Uri, Uri> rewriter)
            => UrlRewriters.Add(hostname, rewriter);
        private Uri Rewrite(Uri url)
        {
            if (UrlRewriters.TryGetValue(url.Host, out var rewriter))
                return rewriter(url);
            return url;
        }

        private class PendingRequest
        {
            public readonly CleanUrl Url;
            public readonly PendingOperation Operation;

            public PendingRequest(CleanUrl url)
            {
                Url = url;
                Operation = PendingOperation.New();
            }
            public PendingOperation Attach()
                => Operation.Attach();
        }
        private class HostnameRequests
        {
            public bool Processing = false;
            public List<PendingRequest> Requests;

            public HostnameRequests(PendingRequest request)
            {
                Requests = new List<PendingRequest>(1) { request };
            }

            public bool ContainsAny(CleanUrl url, out PendingRequest request)
                => Requests.ContainsAny(r =>
                    r.Url.AbsoluteUrlWithoutFragment.Equals(url.AbsoluteUrlWithoutFragment, StringComparison.OrdinalIgnoreCase),
                    out request);
        }

        // We'll pretend urls are entirely case insensitive :)
        private readonly HashSet<string> DnsUnresolvableHostNames;
        private readonly HashSet<string> KnownGoodHostNames;
        private readonly Dictionary<string, HostnameRequests> PendingRequestsByHostname;
        private readonly Dictionary<string, PendingRequest> PendingRequests;
        private readonly Dictionary<string, PendingRequest> RequestsInProgress;
        private readonly Dictionary<string, SiteInfo> CachedSites;

        public SiteCacheState TryGetSiteInfo(Uri url, out SiteInfo info)
        {
            if (!Enabled)
            {
                info = null;
                return SiteCacheState.WebIODisabled;
            }

            url = Rewrite(url);
            CleanUrl cleanUrl = new CleanUrl(url);

            lock (_lock)
            {
                if (CachedSites.TryGetValue(cleanUrl.AbsoluteUrlWithoutFragment, out info))
                    return SiteCacheState.Cached;

                info = new SiteInfo(cleanUrl);

                if (url.HostNameType == UriHostNameType.Dns && DnsUnresolvableHostNames.Contains(url.Host))
                    return SiteCacheState.UnresolvableHostname;

                return SiteCacheState.Unknown;
            }
        }
        public PendingOperation RequestSiteInfo(Uri url)
        {
            if (!Enabled)
                return PendingOperation.Completed();

            url = Rewrite(url);
            CleanUrl cleanUrl = new CleanUrl(url);

            lock (_lock)
            {
                if (RequestsInProgress.TryGetValue(cleanUrl.AbsoluteUrlWithoutFragment, out var request))
                    return request.Attach();

                if (PendingRequests.TryGetValue(cleanUrl.AbsoluteUrlWithoutFragment, out request))
                    return request.Attach();

                if (url.HostNameType == UriHostNameType.Dns)
                {
                    // Is the hostname still not processed?
                    if (PendingRequestsByHostname.TryGetValue(url.Host, out var requests))
                    {
                        // The same uri is already in queue
                        if (requests.ContainsAny(cleanUrl, out request))
                            return request.Attach();

                        var pendingHostnameRequest = new PendingRequest(cleanUrl);
                        requests.Requests.Add(pendingHostnameRequest);
                        return pendingHostnameRequest.Operation;
                    }
                    else if (!KnownGoodHostNames.Contains(url.Host))
                    {
                        // First time we're seing this hostname - add to to PendingRequestsByHostname
                        var newPendingHostnameRequest = new PendingRequest(cleanUrl);
                        PendingRequestsByHostname.Add(url.Host, new HostnameRequests(newPendingHostnameRequest));
                        SchedulerMRE.Set();
                        return newPendingHostnameRequest.Operation;
                    }
                }

                // This would only happend if the site was processed in between calls to TryGetInfo and RequestSiteInfo
                if (CachedSites.ContainsKey(cleanUrl.AbsoluteUrlWithoutFragment))
                    return PendingOperation.Completed();

                var pendingRequest = new PendingRequest(cleanUrl);
                PendingRequests.Add(cleanUrl.AbsoluteUrlWithoutFragment, pendingRequest);
                // Signal to the worker thread that there is work to be done
                SchedulerMRE.Set();

                return pendingRequest.Operation;
            }
        }

        private readonly object _lock = new object();
        private readonly ManualResetEvent SchedulerMRE;
        private readonly int MaxConcurrency;
        private int WorkerCount = 0;

        private void SchedulerWorkInternal()
        {
            while (true)
            {
                // Wait for some work to pop up
                SchedulerMRE.WaitOne();
                lock (_lock)
                {
                    // Are all the workers busy?
                    // If they are, go back to waiting for signals
                    // Work instances also signal the scheduler when they are finished
                    int idleWorkers = MaxConcurrency - WorkerCount;
                    if (idleWorkers != 0)
                    {
                        // Check for pending hostnames first
                        int pendingHostnames = PendingRequestsByHostname.Count;
                        if (pendingHostnames != 0)
                        {
                            int instancesToSchedule = Math.Min(idleWorkers, pendingHostnames);
                            var requests = new KeyValuePair<string, HostnameRequests>[instancesToSchedule];

                            // Take the first instancesToSchedule items from the dictionary
                            var entries = PendingRequestsByHostname.GetEnumerator();
                            for (int i = 0; i < instancesToSchedule; i++)
                            {
                                if (entries.MoveNext())
                                {
                                    var request = entries.Current;
                                    if (request.Value.Processing) i--;
                                    else
                                    {
                                        request.Value.Processing = true;
                                        requests[i] = request;
                                    }
                                }
                                else
                                {
                                    instancesToSchedule = i;
                                    break;
                                }
                            }
                            entries.Dispose();

                            // Update the WorkerCount
                            idleWorkers -= instancesToSchedule;
                            WorkerCount += instancesToSchedule;

                            // Schedule work instance tasks
                            for (int i = 0; i < instancesToSchedule; i++)
                            {
                                var request = requests[i];
                                Task.Run(() => DnsWorkInstanceInternal(request.Key, request.Value.Requests));
                            }
                        }

                        // Is there any work to be done?
                        // If we were signaled by a work instance, there might not be any more work
                        int pendingRequests = PendingRequests.Count;
                        if (pendingRequests != 0 && idleWorkers != 0)
                        {
                            // We can schedule more work instances
                            int instancesToSchedule = Math.Min(idleWorkers, pendingRequests);
                            var requests = new PendingRequest[instancesToSchedule];

                            // Take the first instancesToSchedule items from the dictionary
                            var entries = PendingRequests.GetEnumerator();
                            for (int i = 0; i < instancesToSchedule; i++)
                            {
                                entries.MoveNext();
                                requests[i] = entries.Current.Value;
                            }
                            entries.Dispose();

                            // Move these requests form Pending to InProgress
                            for (int i = 0; i < instancesToSchedule; i++)
                            {
                                var request = requests[i];
                                PendingRequests.Remove(request.Url.AbsoluteUrlWithoutFragment);
                                RequestsInProgress.Add(request.Url.AbsoluteUrlWithoutFragment, request);
                            }

                            // Update the WorkerCount
                            WorkerCount += instancesToSchedule;

                            // Schedule work instance tasks
                            for (int i = 0; i < instancesToSchedule; i++)
                            {
                                var request = requests[i];
                                Task.Run(() => WorkInstanceInternal(request));
                            }
                        }
                    }

                    // Go back to waiting
                    SchedulerMRE.Reset();
                }
            }
        }

        private void DnsWorkInstanceInternal(string host, List<PendingRequest> requests)
        {
            // We start in an unlocked context
            // We are guaranteed to be the only instance working on this hostname

            bool isResolvable;
            try
            {
                Dns.GetHostAddresses(host);
                isResolvable = true;
            }
            catch
            {
                isResolvable = false;
            }

            lock (_lock)
            {
                PendingRequestsByHostname.Remove(host);

                if (isResolvable)
                {
                    KnownGoodHostNames.Add(host);
                    foreach (var request in requests)
                    {
                        PendingRequests.Add(request.Url.AbsoluteUrlWithoutFragment, request);
                    }
                }
                else
                {
                    DnsUnresolvableHostNames.Add(host);
                    foreach (var request in requests)
                    {
                        request.Operation.SignalCompleted();
                    }
                }

                WorkerCount--;
                SchedulerMRE.Set();
            }
        }
        private void WorkInstanceInternal(PendingRequest request)
        {
            // We start in an unlocked context
            // At this point this request is located in RequestsInProgress
            // We are guaranteed to be the only instance working on this exact url (case-insensitive match)
            // We are also guaranteed that this url is either targetting an IP address, or a resolvable hostname

            CleanUrl cleanUrl = request.Url;
            Uri url = cleanUrl.Url;
            SiteInfo siteInfo = new SiteInfo(cleanUrl);

            try
            {
                using (var httpRequest = new HttpRequestMessage(HttpMethod.Get, cleanUrl.AbsoluteUrlWithoutFragment)) // ToDo: Use HEAD instead?
                using (var response = HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    siteInfo.StatusCode = response.StatusCode;
                    siteInfo.Is2xxStatusCode = response.IsSuccessStatusCode;
                    siteInfo.Headers = response.Headers;
                    siteInfo.ContentHeaders = response.Content.Headers;
                    siteInfo.HttpVersion = response.Version;

                    if ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400)
                    {
                        siteInfo.IsRedirect = true;
                        Uri redirectUrl = response.Headers.Location;
                        if (!redirectUrl.IsAbsoluteUri)
                            redirectUrl = new Uri(url.GetLeftPart(UriPartial.Authority) + redirectUrl);
                        siteInfo.RedirectTarget = new CleanUrl(redirectUrl);
                    }
                    else if (siteInfo.ContentType != null && TryGetDownloadableContentType(url.Host, siteInfo.ContentType, out bool isText))
                    {
                        siteInfo.ContentPresent = true;
                        siteInfo.ContentIsText = isText;
                        if (isText)
                        {
                            siteInfo.ContentText = response.Content.ReadAsStringAsync().Result;
                        }
                        else
                        {
                            siteInfo.ContentBytes = response.Content.ReadAsByteArrayAsync().Result;
                        }
                    }
                }
            }
            catch (AggregateException ae) when (ae.InnerException is TaskCanceledException taskCanceledException)
            {
                siteInfo.RequestTimedOut = true;
            }
            catch (AggregateException ae) when (ae.InnerException is HttpRequestException httpException)
            {
                siteInfo.RequestFailed = true;
                siteInfo.HttpException = httpException;
            }

            lock (_lock)
            {
                CachedSites.Add(cleanUrl.AbsoluteUrlWithoutFragment, siteInfo);
                request.Operation.SignalCompleted();
                WorkerCount--;
                SchedulerMRE.Set();
            }
        }
    }
}
