using ReferenceValidator.Parser;
using System.Collections.Generic;

namespace ReferenceValidator
{
    class Report
    {
        public Report(string rootDirectory, List<string> files, int markdownFiles)
        {
            RootDirectory = rootDirectory;
            IndexedFiles = files;
            MarkdownFiles = markdownFiles;
        }

        public class Warning
        {
            public readonly bool IsError;
            public readonly string Message;

            public Warning(bool error, MarkdownEntity entity, string title, string message, string detailedDescription)
            {
                IsError = error;

                string location = entity.Span.IsOneLine
                   ? $"line {entity.Span.Start}"
                   : $"lines {entity.Span.Start}-{entity.Span.End}";

                message = message == null
                    ? string.Empty
                    : " " + message;

                detailedDescription = detailedDescription == null
                    ? string.Empty
                    : " " + detailedDescription;

                Message = $"{title} `{entity.Value}` in `{entity.File.RelativeName}` on {location}{message}.{detailedDescription}";
            }
        }

        public readonly string RootDirectory;
        public int AllFiles => IndexedFiles.Count;
        public readonly int MarkdownFiles;
        public readonly List<string> IndexedFiles;
        public bool HasErrors { get; private set; } = false;
        public readonly List<Warning> Warnings = new List<Warning>();

        public int TotalEntityCount = 0;
        public int TotalLines = 0;

        private readonly object _lock = new object();
        public void Add(MarkdownEntity entity, WarningType type, string detailedDescription = null)
        {
            bool isError = ErrorTypes.Contains(type);
            if (isError) HasErrors = true;

            string title;
            string message = null;

            switch (type)
            {
                case WarningType.EmptyFragmentIdentifier:
                    title = "Fragment identifier";
                    message = "is empty";
                    break;

                case WarningType.MultipleHashtagsInReference:
                    title = "Reference";
                    message = "contains multiple hashtags";
                    break;

                case WarningType.MultipleSpacesInHeader:
                    title = "Header";
                    message = "contains multiple joint spaced";
                    break;

                case WarningType.PossibleUnresolvedReference:
                    title = "Possible unresolved reference";
                    detailedDescription = "Add <!-- ignore --> to the line to make the verification engine ignore it";
                    break;

                case WarningType.UndefinedNamedReference:
                    title = "Unresolved reference name";
                    break;

                case WarningType.UnusedNamedReference:
                    title = "Unused named reference";
                    break;

                case WarningType.UnresolvedReference:
                    title = "Unresolved reference";
                    break;

                case WarningType.EmptySquareBrackets:
                    title = "Empty square brackets";
                    break;

                case WarningType.Link_ConnectionRefused:
                    title = "Connection refused to";
                    break;

                case WarningType.Link_Invalid:
                    title = "Invalid link";
                    break;

                case WarningType.Link_Redirect:
                    title = "Link";
                    message = "returned a redirect chain";
                    break;

                case WarningType.Link_TooManyRedirects:
                    title = "Link";
                    message = "returned a redirect too many times";
                    break;

                case WarningType.Link_SiteReturnedErrorCode:
                    title = "Link";
                    message = "returned an error status code";
                    break;

                case WarningType.Link_FragmentIdentifierNotDefined:
                    title = "Link";
                    message = "references a fragment identifier that is not defined on site";
                    break;

                case WarningType.Html_BrokenTag:
                    title = "Possible broken html tag";
                    break;

                case WarningType.Html_UnmatchedTag:
                    title = "Unmatched html tags for";
                    break;

                case WarningType.InternalError:
                    title = "Internal error occured";
                    message = "--Please contact the project author (t.me/MihaZupan), giving him the whole error message--";
                    break;

                default:
                    title = "INTERNAL ERROR - MISSING WARNING DESCRTIPTION";
                    message = $"--Warning that triggered this: {type.ToString()}--";
                    detailedDescription = null;
                    break;
            }

            Warning warning = new Warning(isError, entity, title, message, detailedDescription);
            lock (_lock) Warnings.Add(warning);
        }

        public static readonly HashSet<WarningType> ErrorTypes = new HashSet<WarningType>()
        {
            WarningType.InternalError,
            WarningType.EmptyFragmentIdentifier,
            WarningType.Link_ConnectionRefused,
            WarningType.Link_Invalid,
            WarningType.Link_FragmentIdentifierNotDefined,
            WarningType.Link_SiteReturnedErrorCode,
            WarningType.Link_TooManyRedirects,
            WarningType.Html_UnmatchedTag,
            WarningType.MultipleHashtagsInReference,
            WarningType.MultipleSpacesInHeader,
            WarningType.UndefinedNamedReference,
            WarningType.UnresolvedReference,
        };
    }
}
