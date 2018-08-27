# MarkdownReferenceValidator
A dotnet program that tests all file/fragment/link references in .md files

## Usage

ReferenceValidator [rootDirectory] [reportPath]
* `rootDirectory` is the directory that contains markdown files, defaults to `WORKDIR\src`
* `reportPath` is the location for the report json, defaults to `WORKDIR\report.json`

Both arguments are optional.

You can disable generating the report json file by passing `noreport` as `reportPath`.

## Errors

If the validator cought something it shouldn't,
or it missed something terrible,
please open an issue on this repository.