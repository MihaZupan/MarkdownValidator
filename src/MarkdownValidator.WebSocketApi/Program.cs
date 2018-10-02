﻿/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig;
using MihaZupan.MarkdownValidator;
using MihaZupan.MarkdownValidator.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace MarkdownValidator.WebSocketApi
{
    class Program
    {
        private static MarkdownContextValidator Validator;
        private static WebSocketServer Server;

        static void Main(string[] args)
        {
            Validator = new MarkdownContextValidator(new Config(""));
            Validator.AddMarkdownFile("Test.md", "");
            Server = new WebSocketServer(6300, false);
            Server.AddWebSocketService<MarkdownBehaviour>("/");
            Server.Start();
            while (true) Thread.Sleep(10000);
        }

        class MarkdownBehaviour : WebSocketBehavior
        {
            CancellationTokenSource _source = new CancellationTokenSource();
            readonly ManualResetEvent MRE = new ManualResetEvent(true);
            private CancellationToken _token;

            protected override void OnMessage(MessageEventArgs e)
            {
                _source.Cancel();
                _source = new CancellationTokenSource();

                MRE.WaitOne();
                MRE.Reset();
                var _token = _source.Token;

                Task.Run(() =>
                {
                    try
                    {
                        var request = JsonConvert.DeserializeObject<MarkdownRequest>(e.Data);

                        if (!_token.IsCancellationRequested)
                            HandleRequest(request);
                    }
                    finally
                    {
                        MRE.Set();
                    }
                });
            }

            private void HandleRequest(MarkdownRequest request)
            {
                var html = Markdown.ToHtml(request.Markdown);

                while (!_token.IsCancellationRequested)
                {
                    ValidationReport report;
                    lock (Validator)
                    {
                        Validator.UpdateMarkdownFile("Test.md", request.Markdown);
                        report = Validator.Validate();
                    }

                    if (_token.IsCancellationRequested)
                        return;

                    Send(request.Version, html, report);

                    if (report.IsComplete || _token.IsCancellationRequested)
                        return;

                    Task.Delay(250).Wait(_token);
                }
            }

            private void Send(int version, string html, ValidationReport report)
            {
                WarningSlim[] warnings;
                if (report.WarningCount != 0)
                {
                    warnings = new WarningSlim[report.WarningCount];
                    var reportWarnings = report.WarningsByFile.Values.Single();
                    for (int i = 0; i < warnings.Length; i++)
                    {
                        var reportWarning = reportWarnings[i];
                        warnings[i] = new WarningSlim()
                        {
                            Line = reportWarning.Location.StartLine + 1,
                            Message = reportWarning.Message
                        };
                    }
                }
                else warnings = Array.Empty<WarningSlim>();

                var markdownResponse = new MarkdownResponse()
                {
                    Html = html,
                    Warnings = warnings,
                    Version = version
                };
                string responseJson = JsonConvert.SerializeObject(markdownResponse, Formatting.Indented);
                Send(responseJson);
            }
        }

        class WarningSlim
        {
            public int Line;
            public string Message;
        }
        class MarkdownRequest
        {
            public string Markdown;
            public int Version;
        }
        class MarkdownResponse
        {
            public string Html;
            public WarningSlim[] Warnings;
            public int Version;
        }
    }
}