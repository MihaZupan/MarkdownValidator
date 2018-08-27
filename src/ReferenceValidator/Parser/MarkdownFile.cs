using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ReferenceValidator.Parser
{
    class MarkdownFile
    {
        public readonly string FullPath;
        public readonly string RelativeName;
        public readonly string RelativeLowercaseName;
        public readonly string RelativeLowercaseDirectory;
        public readonly int EntityCount;
        public readonly int LineCount;

        public readonly List<MarkdownEntity> DefinedNamedReferences = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> AccessedNamedReferences = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> DefinedHeaders = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> References = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> ExternalLinks = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> EmptySquareBracketsEntities = new List<MarkdownEntity>();
        public readonly List<MarkdownEntity> HtmlTagWarnings = new List<MarkdownEntity>();

        private MarkdownFile(string filePath, string relativeName, string relativeLowercaseName, List<MarkdownEntity> entities, int lineCount)
        {
            FullPath = filePath;
            RelativeName = relativeName;
            RelativeLowercaseName = relativeLowercaseName;
            RelativeLowercaseDirectory = Path.GetDirectoryName(RelativeLowercaseName).Replace('\\', '/');
            EntityCount = entities.Count;
            LineCount = lineCount;

            foreach (var entity in entities)
            {
                entity.File = this;
                switch (entity.Type)
                {
                    case MarkdownEntityType.EmptySquareBracketsEntity:
                        EmptySquareBracketsEntities.Add(entity);
                        break;

                    case MarkdownEntityType.AccessedNamedReference:
                    case MarkdownEntityType.AccessedSameNameReference:
                        AccessedNamedReferences.Add(entity);
                        break;

                    case MarkdownEntityType.DefinedNamedReference:
                        DefinedNamedReferences.Add(entity);
                        break;

                    case MarkdownEntityType.Header:
                        DefinedHeaders.Add(entity);
                        break;

                    case MarkdownEntityType.BrokenHtmlTag:
                    case MarkdownEntityType.UnmatchedHtmlTags:
                        HtmlTagWarnings.Add(entity);
                        break;

                    case MarkdownEntityType.Reference:
                        string reference = entity.CleanValue;
                        if (reference.OrdinalContains("..."))
                            continue;
                        References.Add(entity);
                        break;

                    case MarkdownEntityType.ExternalLink:
                        string path = entity.CleanValue;
                        if (path.OrdinalContains("...")         ||  // http://...
                            path.OrdinalContains("example.org") ||  // RFC2606
                            path.OrdinalContains("example.com") ||  // RFC2606
                            path.OrdinalContains("example.net") ||  // RFC2606
                            path.OrdinalContains(".example")    ||  // RFC2606
                            path.OrdinalContains("2001:DB8::")  ||  // RFC3849
                            path.OrdinalContains("192.0.2.")    ||  // RFC5737
                            path.OrdinalContains("198.51.100.") ||  // RFC5737
                            path.OrdinalContains("203.0.113.")  ||  // RFC5737
                            (path.Contains('<') && path.Contains('>'))) // e.g. something.com/bot<token>/<file_path>
                            continue;
                        ExternalLinks.Add(entity);
                        break;
                }
            }
        }

        //private static readonly Regex InlineMonoTextRegex =
        //    new Regex(@"[^\[]?`[^\]].*?[^\[]`[^\]]", RegexOptions.Compiled);
        private static readonly Regex ExternalLinkRegex =
            new Regex(@"https?:\/\/[\S]+?\.[^\s\)\n\*\""]+", RegexOptions.Compiled);
        private static readonly Regex ReferenceRegex =
            new Regex(@"(?:[^\]]|^)\[([^\[\]\n]+?)\](?:[^\[\(:]|\(([^\:]+?)\)|$)", RegexOptions.Compiled);
        private static readonly Regex NoteBlockLeakingNamedReferenceRegex = // This won't catch everything, it stops at dumb levels of ugly
            new Regex(@"[^\[\]\n]*?\[([^\[\]\n]*?)\n[\S]*?](?:\[(.*?)])?", RegexOptions.Compiled);
        private static readonly Regex HeaderRegex =
            new Regex(@"^[\s]*?#+? (.*?)$", RegexOptions.Compiled);
        private static readonly Regex NamedReferenceRegex =
            new Regex(@"\[[^\]\[]+?]\[([^\[\]]+?)]", RegexOptions.Compiled);
        private static readonly Regex EmptySquareBracketsRegex =
            new Regex(@"\[[\s]*?\]", RegexOptions.Compiled);
        private static readonly Regex NamedReferenceDefinitionRegex =
            new Regex(@"\[([^\[\]]+?)\]:\s+?<?(?:\S+:\S+|([^\[\]\:\s\>]+)|\[.+?]\((?:([^:]+?)|.+?)\))", RegexOptions.Compiled);
        private static readonly Regex HtmlCommentRegex =
            new Regex(@"<!--.*-->", RegexOptions.Compiled);
        private static readonly Regex IgnoreLineCommentRegex = // Parsing engine ignores lines that contain this comment
            new Regex(@"<!--\s*?ignore\s*?-->", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static MarkdownFile Parse(string filePath, string relativePath, string relativeLowercaseName)
        {
            List<MarkdownEntity> entities = new List<MarkdownEntity>();
            string[] fileLines = File.ReadAllLines(filePath);

            bool hasSquareBrackets = false;
            bool inCodeBlock = false;
            LineSpan location = null;
            string line = null;
            List<(int, bool)> detailsBlocks = new List<(int, bool)>();

            // Helper methods that make saving entities easier in actual parsing code
            void addAccessedNamedReference(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.AccessedNamedReference, location, value));
            void addAccessedSameNameReference(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.AccessedSameNameReference, location, value));
            void addDefinedNamedReference(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.DefinedNamedReference, location, value));
            void addHeader(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.Header, location, value));
            void addReference(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.Reference, location, value));
            void addExternalLink(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.ExternalLink, location, value));
            void addEmptySquareBrackets()
                => entities.Add(new MarkdownEntity(MarkdownEntityType.EmptySquareBracketsEntity, location, ""));
            void addBrokenHtmlTag(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.BrokenHtmlTag, location, value));
            void addUnmatchedHtmlTags(string value)
                => entities.Add(new MarkdownEntity(MarkdownEntityType.UnmatchedHtmlTags, location, value));
            void testHtmlTags(string tagName, List<(int lineNr, bool isOpen)> positions)
            {
                if (positions.Count == 0)
                    return;

                location = new LineSpan(positions[0].lineNr, positions[0].lineNr);

                int open = 0;
                foreach (var (lineNr, isOpen) in positions)
                {
                    location = new LineSpan(location.Start, lineNr);
                    if (isOpen) open++;
                    else
                    {
                        open--;
                        if (open < 1)
                        {
                            location = new LineSpan(lineNr, lineNr);
                            if (open == -1) break;
                        }
                    }
                }
                if (open != 0)
                    addUnmatchedHtmlTags(tagName);
            }

            // Called for each line (except for code blocks)
            void processLine()
            {
                if (!hasSquareBrackets)
                    return;

                foreach (Match match in ReferenceRegex.Matches(line))
                {
                    if (match.Groups[2].Success)
                        addReference(match.Groups[2].Value);
                    else
                        addAccessedSameNameReference(match.Groups[1].Value);
                }                    

                foreach (Match match in NamedReferenceRegex.Matches(line))
                {
                    addAccessedNamedReference(match.Groups[1].Value);
                    if (match.Groups[2].Success)
                        addReference(match.Groups[2].Value);
                }

                foreach (Match match in NamedReferenceDefinitionRegex.Matches(line))
                {
                    addDefinedNamedReference(match.Groups[1].Value);
                    if (match.Groups[2].Success)
                        addReference(match.Groups[2].Value);
                    if (match.Groups[3].Success)
                        addReference(match.Groups[3].Value);
                }
            }

            for (int i = 0; i < fileLines.Length; i++)
            {
                line = fileLines[i];
                location = new LineSpan(i + 1, i + 1);
                hasSquareBrackets = line.Contains('[');

                // Fast-tract empty lines - 3 characters is the shortest meaningful thing `[a]` or `# a`
                if (line.Length < 3)
                    continue;

                // Check for html comments
                if (IgnoreLineCommentRegex.Match(line).Success)
                    continue;

                line = HtmlCommentRegex.Replace(line, "");

                if (line.StartsWith("```", StringComparison.Ordinal))
                    inCodeBlock = !inCodeBlock;

                foreach (Match match in ExternalLinkRegex.Matches(line))
                    addExternalLink(match.Value);

                if (inCodeBlock)
                {
                    continue;
                }
                else
                {
                    if (hasSquareBrackets && EmptySquareBracketsRegex.Match(line).Success)
                        addEmptySquareBrackets();
                }

                Match headerMatch = HeaderRegex.Match(line);
                if (headerMatch.Success)
                {
                    addHeader(headerMatch.Groups[1].Value);
                    continue; // A header line can't be anything else at the same time
                }

                if (line.OrdinalContains("details>"))
                {
                    if (line.OrdinalContains("<details>"))
                        detailsBlocks.Add((location.Start, true));
                    else if (line.OrdinalContains("</details>"))
                        detailsBlocks.Add((location.Start, false));
                    else
                        addBrokenHtmlTag("details>");
                }

                if (line.StartsWith('>'))
                {
                    hasSquareBrackets = true; // Override
                    List<string> lines = new List<string>(4);
                    lines.Add(line);
                    i++;
                    for (; i < fileLines.Length; i++)
                    {
                        string nextLine = fileLines[i];
                        lines.Add(nextLine);
                        line += nextLine;
                        if (nextLine.Length == 0)
                            break;
                    }
                    foreach (Match match in NoteBlockLeakingNamedReferenceRegex.Matches(line))
                    {
                        if (match.Groups[2].Success)
                            addAccessedNamedReference(match.Groups[2].Value);
                        else
                            addAccessedSameNameReference(match.Groups[1].Value);
                    }
                    foreach (string newLine in lines)
                    {
                        line = newLine;
                        processLine();
                    }
                }
                else
                {
                    processLine();
                }
            }

            testHtmlTags("details", detailsBlocks);

            return new MarkdownFile(filePath, relativePath, relativeLowercaseName, entities, fileLines.Length);
        }
    }
}
