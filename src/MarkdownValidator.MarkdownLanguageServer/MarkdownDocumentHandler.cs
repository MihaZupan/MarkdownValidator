/*
    Copyright (c) Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Helpers;
using MihaZupan.MarkdownValidator.Warnings;
using Newtonsoft.Json;
using OmniSharp.Extensions.Embedded.MediatR;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Server.Capabilities;

namespace MihaZupan.MarkdownValidator.MarkdownLanguageServer
{
    class MarkdownDocumentHandler : ITextDocumentSyncHandler, IDidChangeWatchedFilesHandler
    {
        public TextDocumentSyncKind Change => TextDocumentSyncKind.Full;
        private static readonly DocumentSelector _documentSelector = new DocumentSelector(
            new DocumentFilter()
            {
                Pattern = "**/*.md"
            });

        public TextDocumentChangeRegistrationOptions GetRegistrationOptions()
        {
            return new TextDocumentChangeRegistrationOptions()
            {
                DocumentSelector = _documentSelector,
                SyncKind = Change
            };
        }
        public TextDocumentAttributes GetTextDocumentAttributes(Uri uri)
            => new TextDocumentAttributes(uri, "markdown");

        public Task<Unit> Handle(DidChangeTextDocumentParams request, CancellationToken cancellationToken)
        {
            string text = request.ContentChanges.Single().Text;
            string path = PathHelper.GetPathFromFileUri(request.TextDocument.Uri);

            if (text.Length == 0)
            {
                _router.Window.LogWarning("Empty");
            }

            if (!_validator.UpdateMarkdownFile(path, text))
                _validator.AddMarkdownFile(path, text);

            Validate();

            return Unit.Task;
        }
        public Task<Unit> Handle(DidOpenTextDocumentParams request, CancellationToken cancellationToken)
        {
            string text = request.TextDocument.Text;
            string path = PathHelper.GetPathFromFileUri(request.TextDocument.Uri);

            if (!_validator.UpdateMarkdownFile(path, text))
                _validator.AddMarkdownFile(path, text);

            Validate();

            return Unit.Task;
        }
        public Task<Unit> Handle(DidCloseTextDocumentParams request, CancellationToken cancellationToken)
            => Unit.Task;
        public Task<Unit> Handle(DidSaveTextDocumentParams request, CancellationToken cancellationToken)
        {
            string text = request.Text;
            string path = PathHelper.GetPathFromFileUri(request.TextDocument.Uri);

            if (!_validator.UpdateMarkdownFile(path, text))
                _validator.AddMarkdownFile(path, text);

            Validate();

            return Unit.Task;
        }
        public Task<Unit> Handle(DidChangeWatchedFilesParams request, CancellationToken cancellationToken)
        {
            bool deleted = false;
            foreach (var change in request.Changes)
            {
                if (change.Type == FileChangeType.Deleted)
                {
                    _validator.RemoveEntity(PathHelper.GetPathFromFileUri(change.Uri));
                    deleted = true;
                    break;
                }
            }

            if (deleted)
                Validate();

            return Unit.Task;
        }

        public void SetCapability(SynchronizationCapability capability) { }
        public void SetCapability(DidChangeWatchedFilesCapability capability) { }

        TextDocumentRegistrationOptions IRegistration<TextDocumentRegistrationOptions>.GetRegistrationOptions()
            => new TextDocumentRegistrationOptions() { DocumentSelector = _documentSelector };
        TextDocumentSaveRegistrationOptions IRegistration<TextDocumentSaveRegistrationOptions>.GetRegistrationOptions()
            => new TextDocumentSaveRegistrationOptions() { DocumentSelector = _documentSelector, IncludeText = true };
        object IRegistration<object>.GetRegistrationOptions() => null;


        public const string ConfigurationFileName = ".markdown-validator.json";

        private readonly ILanguageServer _router;
        private readonly MarkdownContextValidator _validator;

        public MarkdownDocumentHandler(ILanguageServer router)
        {
            _router = router;

            Config configuration = null;

            string path = ConfigurationFileName;
            if (File.Exists(path))
            {
                configuration = TryLoadConfiguration(path);
            }
            if (configuration != null)
            {
                path = Path.Combine(Path.GetDirectoryName(Environment.CurrentDirectory), ConfigurationFileName);
                if (File.Exists(path))
                {
                    configuration = TryLoadConfiguration(path);
                }
            }
            if (configuration != null)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationFileName);
                if (File.Exists(path))
                {
                    configuration = TryLoadConfiguration(path);
                }
            }

            if (configuration == null) configuration = new Config(string.Empty);
            else configuration.RootWorkingDirectory = string.Empty;
            _validator = new MarkdownContextValidator(configuration);

            List<string> files = new List<string>();
            Stack<string> directories = new Stack<string>();
            directories.Push(Environment.CurrentDirectory);
            while (directories.Count > 0)
            {
                string directory = directories.Pop();
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    directories.Push(dir);
                    _validator.AddEntity(dir);
                }
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (IsMarkdownFile(file))
                    {
                        files.Add(file);
                    }
                    else
                    {
                        _validator.AddEntity(file);
                    }
                }
            }

            Task.Run(() =>
            {
                Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 2 },
                    file =>
                    {
                        string source = File.ReadAllText(file);
                        _validator.AddMarkdownFile(file, source);
                    });
            });

            RevalidationTimer = new Timer(s => Validate());
        }
        private static bool IsMarkdownFile(string path)
            => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
        private Config TryLoadConfiguration(string path)
        {
            string json = File.ReadAllText(path);
            try
            {
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch
            {
                WasMarkdownConfigurationInvalid = path;
                return null;
            }
        }
        private string WasMarkdownConfigurationInvalid = null;

        private readonly Timer RevalidationTimer;
        private ValidationReport PreviousReport = ValidationReport.Empty;
        private void Validate()
        {
            lock (RevalidationTimer)
            {
                if (WasMarkdownConfigurationInvalid != null)
                {
                    WasMarkdownConfigurationInvalid = Path.GetFullPath(WasMarkdownConfigurationInvalid);
                    _router.Window.ShowError($"Invalid configuration format at `{WasMarkdownConfigurationInvalid}`");
                    string defaultConfigName =
                        Path.GetFileNameWithoutExtension(ConfigurationFileName) + "-default" + Path.GetExtension(ConfigurationFileName);
                    string path = Path.Combine(Path.GetDirectoryName(WasMarkdownConfigurationInvalid), defaultConfigName);
                    File.WriteAllText(path, JsonConvert.SerializeObject(new Config(null), Formatting.Indented));
                    WasMarkdownConfigurationInvalid = null;
                }

                var report = _validator.Validate();

                if (report.IsComplete) RevalidationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                else RevalidationTimer.Change(Math.Max(report.SuggestedWait, 100), Timeout.Infinite);

                if (report == PreviousReport)
                    return;

                var diff = report.Diff(PreviousReport);
                PreviousReport = report;

                foreach (var affectedFile in diff.AffectedFiles)
                {
                    string path;
                    Diagnostic[] fileDiagnostics;

                    if (report.WarningsByFile.TryGetValue(affectedFile.Key, out List<Warning> warnings))
                    {
                        path = warnings.First().Location.FullFilePath;
                        path = Path.Combine(report.Configuration.RootWorkingDirectory, affectedFile.Key);
                        fileDiagnostics = new Diagnostic[warnings.Count];
                        for (int i = 0; i < fileDiagnostics.Length; i++)
                        {
                            var warning = warnings[i];
                            int line = Math.Max(warning.Location.StartLine, 0);
                            fileDiagnostics[i] = new Diagnostic()
                            {
                                Code = new DiagnosticCode(warning.ID.Identifier),
                                Range = ToRange(warning.Location),
                                Message = warning.Message,
                                Severity = ToDiagnosticSeverity(warning),
                                //Source = "MarkdownValidator"
                            };
                        }
                    }
                    else
                    {
                        path = Path.Combine(report.Configuration.RootWorkingDirectory, affectedFile.Key);
                        fileDiagnostics = Array.Empty<Diagnostic>();
                    }

                    if (path[0] != '/') // It is already there on linux, has to be added on windows
                        path = '/' + path;

                    _router.Document.PublishDiagnostics(new PublishDiagnosticsParams()
                    {
                        Uri = new Uri("file://" + path),
                        Diagnostics = new Container<Diagnostic>(fileDiagnostics)
                    });
                }
            }
        }
        private static DiagnosticSeverity ToDiagnosticSeverity(Warning warning)
        {
            if (warning.IsError) return DiagnosticSeverity.Error;
            return DiagnosticSeverity.Warning;
        }
        private static Range ToRange(WarningLocation location)
        {
            if (!location.RefersToEntireFile)
            {
                return new Range(
                    new Position(location.StartLine, location.StartLineColumn),
                    new Position(location.EndLine, location.EndLineColumn));
            }
            else
            {
                return new Range(new Position(0, 0), new Position(0, 0));
            }
        }
    }
}
