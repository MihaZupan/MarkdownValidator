# Markdown Validator

[![Build Status](https://travis-ci.org/MihaZupan/MarkdownValidator.svg?branch=master)](https://travis-ci.org/MihaZupan/MarkdownValidator)
[![Build status](https://ci.appveyor.com/api/projects/status/kpq6enso8ilo87sw/branch/master?svg=true)](https://ci.appveyor.com/project/MihaZupan/markdownvalidator/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/MihaZupan/MarkdownValidator/badge.svg?branch=master)](https://coveralls.io/github/MihaZupan/MarkdownValidator?branch=master)

A lightning fast, [CommonMark] compliant, extensible .NET Core library and a set of derived tools for markdown context analysis and validation.

* A [Visual Studio Code extension]
* Standalone tool
  * Looks for file system changes and emits warnings whenever you save a file
* POC test site: [markdown-validator.ml](http://markdown-validator.ml)

**ToDo**
* Visual Studio extension
* Docker image that is simple to deploy and use with CI

### License

This project uses [Markdig] internally, therefore, its license also applies.
See the [LICENSE](LICENSE) for more information (both licenses are BSD 2-Clause).

[CommonMark]: https://commonmark.org/
[Markdig]: https://github.com/lunet-io/markdig
[Visual Studio Code extension]: https://marketplace.visualstudio.com/items?itemName=MihaZupan.markdown-validator
