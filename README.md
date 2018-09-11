# Markdown Validator

Is an efficient .NET library and a set of derived tools for finding broken links,
broken intra and inter-file references (links to headings, other files/directories in the project)
and other useful warnings in projects that make use of markdown.

Unlike other tools, that check the links after markdown is already converted into html,
this one works with raw markdown.

* Standalone tool
  * comes with an option to look for file system changes and update warnings whenever you save a file

**ToDo** 
* Visual Studio extension that gives you warnings in real-time
* Docker image that is simple to deploy and use with CI
