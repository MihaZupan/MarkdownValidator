'use strict';

import { ExtensionContext, window, commands, workspace, TreeDataProvider, TreeItem, OpenDialogOptions } from 'vscode';
import { LanguageClient, LanguageClientOptions, ServerOptions } from 'vscode-languageclient';
import { exec } from 'child_process';
import { satisfies } from 'semver';

var MarkdownContext: ExtensionContext;
var LanguageServer: LanguageClient;
var CurrentDirectory: string = process.cwd();

export function activate(context: ExtensionContext) {
    MarkdownContext = context;

    exec('dotnet --info', function(error, data) {
        let missingDotnet: boolean = true;
        if (!error) {
            let match = data.match(/Host.*[\r\n]*?.*?Version: (.*?)$/m);
            if (match != null && satisfies(match[1], ">=2.1.0"))
                missingDotnet = false;
        }
        
        if (missingDotnet) showMissingDotnetCoreWarning();
        else startExtension();
    });

    registerChangeDirectoryCommand();
}

function registerChangeDirectoryCommand() {
    commands.registerCommand('markdown_validator/changeWorkingDirectory', async => {
        let options: OpenDialogOptions = {
            "canSelectMany": false,
            "canSelectFiles": false,
            "canSelectFolders": true,
            "openLabel": "Choose a markdown working directory"
        };
        window.showOpenDialog(options).then(folder => {
            if (folder != undefined)
            {
                CurrentDirectory = folder[0].fsPath;
                console.log("Changing Markdown Validator working directory to " + CurrentDirectory);
                LanguageServer.sendRequest('markdown_validator/changeWorkingDirectory', { "NewDirectory": CurrentDirectory });
            }
        });
    })
}

function showMissingDotnetCoreWarning()
{
    const message = 'Failed to find a dotnet core 2.1 installation on the path.'
    const getDotNetMessage = 'Get dotnet core';
    const dotnetUrl = 'https://www.microsoft.com/net/download';

    window.showErrorMessage(message, getDotNetMessage).then(value => {
        if (value == getDotNetMessage) {
            require('opn')(dotnetUrl);
        }
    });

    let outputChannel = window.createOutputChannel("Markdown Validator");
    outputChannel.appendLine(message);
    outputChannel.appendLine(getDotNetMessage + " at " + dotnetUrl);
    outputChannel.show();
}

function startExtension()
{
    let dllPath = MarkdownContext.extensionPath + '/bin/MarkdownValidator.MarkdownLanguageServer.dll';
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
            fileEvents: workspace.createFileSystemWatcher('**', false, true, false)
        },
    }
    
    LanguageServer = new LanguageClient('markdown-validator', 'Markdown Validator', serverOptions, clientOptions);
    let disposable = LanguageServer.start();
    
    window.registerTreeDataProvider('markdown_validator/workingDirView', new MarkdownValidatorTreeViewProvider());
    commands.executeCommand('setContext', 'markdown_validator/active', true);
    
    MarkdownContext.subscriptions.push(disposable);
}

export class MarkdownValidatorTreeViewProvider implements TreeDataProvider<TreeItem> {

    getTreeItem(element) {
		return element;
    }
    
    getChildren()
    {
        let changeDirectory = new TreeItem('Change directory');
        changeDirectory.tooltip = 'Choose a new root directory for the Markdown Validator';
        changeDirectory.command = { title: '', command: 'markdown_validator/changeWorkingDirectory'};

        return Promise.resolve([changeDirectory]);
    }
}