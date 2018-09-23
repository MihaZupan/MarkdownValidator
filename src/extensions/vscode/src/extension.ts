'use strict';

import { window, workspace, ExtensionContext } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient';

export function activate(context: ExtensionContext) {

    const exec = require('child_process').exec;
    const semver = require('semver');
    
    exec('dotnet --version', function(error, data) {
        if (!error && semver.satisfies(data, '>=2.1.0')) startExtension(context);
        else showMissingDotnetCoreWarning();
    });
}

function showMissingDotnetCoreWarning()
{
    const message = 'Failed to find a dotnet core 2.1 installation on the path.'
    const getDotNetMessage = 'Get dotnet core';
    const dotnetUrl = 'https://www.microsoft.com/net/download';

    window.showErrorMessage(message, getDotNetMessage).then(value => {
        if (value == getDotNetMessage) {
            require('open')(dotnetUrl);
        }
    });

    let outputChannel = window.createOutputChannel("Markdown Validator");
    outputChannel.appendLine(message);
    outputChannel.appendLine(getDotNetMessage + " at " + dotnetUrl);
    outputChannel.show();
}

function startExtension(context: ExtensionContext)
{
    let dllPath = context.extensionPath + '\\bin\\MarkdownValidator.MarkdownLanguageServer.dll';
    let serverOptions: ServerOptions = {
        run: { command: 'dotnet', args: [dllPath] },
        debug: { command: 'dotnet', args: [dllPath]
        }
    }
    
    let clientOptions: LanguageClientOptions = {
        documentSelector: [
            {
                pattern: '**/*.md',
            }
        ],
        synchronize: {
            configurationSection: 'markdown_validator',
            fileEvents: workspace.createFileSystemWatcher('**/*.md')
        },
    }
    
    const client = new LanguageClient('markdown-validator', 'Markdown Validator', serverOptions, clientOptions);
    let disposable = client.start();
    
    context.subscriptions.push(disposable);
}