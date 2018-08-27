using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ReferenceValidator.Parser;

namespace ReferenceValidator
{
    static class Validator
    {
        public static Report Validate(string rootDirectory)
        {
            rootDirectory = Path.GetFullPath(rootDirectory);
            int pathPrefixSize = rootDirectory.Length;
            List<string> files = GetAllFiles(rootDirectory, out List<string> subdirectories);

            HashSet<string> localEntities = new HashSet<string>();

            List<MarkdownFile> markdownFiles = new List<MarkdownFile>();
            object _lock = new object();
            Parallel.ForEach(files, new ParallelOptions() { MaxDegreeOfParallelism = 8 },
                file =>
                {
                    string relativeName = file.Substring(pathPrefixSize).Replace('\\', '/').Trim();
                    string relativeNameLower = relativeName.ToLower();
                    lock (_lock) localEntities.Add(relativeName.ToLower());
                    if (file.EndsWith(".md"))
                    {
                        var markdownFile = MarkdownFile.Parse(file, relativeName, relativeNameLower);
                        lock (_lock) markdownFiles.Add(markdownFile);
                    }
                });
            foreach (string dir in subdirectories)
            {
                string relativeLowercaseName = dir.Substring(pathPrefixSize).Replace('\\', '/').ToLower();
                if (localEntities.Contains(relativeLowercaseName + "/readme.md"))
                    localEntities.Add(relativeLowercaseName);
            }

            Report report = new Report(rootDirectory, files, markdownFiles.Count);

            List<MarkdownEntity> externalLinks = new List<MarkdownEntity>();

            // Operations that don't rely on other files being pre-processed
            foreach (var file in markdownFiles)
            {
                report.TotalEntityCount += file.EntityCount;
                report.TotalLines += file.LineCount;

                externalLinks.AddRange(file.ExternalLinks);

                // Put all the headers from this file into a global entity HashSet
                foreach (var header in file.DefinedHeaders)
                {
                    if (header.Value.Contains("--"))
                        report.Add(header, WarningType.MultipleSpacesInHeader);
                    string entityPath = file.RelativeLowercaseName + '#' + header.CleanValue;
                    localEntities.Add(entityPath);
                    if (entityPath.OrdinalContains(".md"))
                        localEntities.Add(entityPath.Replace(".md", "", StringComparison.Ordinal));
                }

                foreach (var emptySquareBracket in file.EmptySquareBracketsEntities)
                    report.Add(emptySquareBracket, WarningType.EmptySquareBrackets);

                // Use a hashset for named references to allow for faster lookups
                HashSet<string> namedReferences = new HashSet<string>(
                    Math.Max(file.DefinedNamedReferences.Count, file.AccessedNamedReferences.Count));

                // Make sure all accessed named references actually exist
                foreach (var name in file.DefinedNamedReferences)
                    namedReferences.Add(name.CleanValue);
                foreach (var name in file.AccessedNamedReferences)
                {
                    if (!namedReferences.Contains(name.CleanValue))
                    {
                        if (name.Type == MarkdownEntityType.AccessedSameNameReference)
                            report.Add(name, WarningType.PossibleUnresolvedReference);
                        else
                            report.Add(name, WarningType.UndefinedNamedReference);
                    }
                }
                namedReferences.Clear();

                // Issue a warning if defined named references aren't being used
                foreach (var name in file.AccessedNamedReferences)
                    namedReferences.Add(name.CleanValue);
                foreach (var name in file.DefinedNamedReferences)
                {
                    if (!namedReferences.Contains(name.CleanValue))
                        report.Add(name, WarningType.UnusedNamedReference);
                }

                // Check html tag warnings
                foreach (var warning in file.HtmlTagWarnings)
                {
                    switch (warning.Type)
                    {
                        case MarkdownEntityType.BrokenHtmlTag:
                            report.Add(warning, WarningType.Html_BrokenTag);
                            break;

                        case MarkdownEntityType.UnmatchedHtmlTags:
                            report.Add(warning, WarningType.Html_UnmatchedTag);
                            break;

                        default:
                            report.Add(warning, WarningType.InternalError, "Default case reached when checking HtmlTagWarnings");
                            break;
                    }
                }
            }

            // Operations that rely on other files being pre-processed
            foreach (var file in markdownFiles)
            {
                foreach (var reference in file.References)
                {
                    string path = reference.CleanValue;
                    int hashtagIndex = path.IndexOf('#');
                    if (hashtagIndex != -1)
                    {                        
                        if (path.EndsWith('#'))
                        {
                            report.Add(reference, WarningType.EmptyFragmentIdentifier);
                            if (path.Length > 1)
                                path = path.Substring(hashtagIndex); // Try to see if the local file exists
                            else continue;
                        }
                        else if (path.Contains('#', hashtagIndex + 1))
                        {
                            report.Add(reference, WarningType.MultipleHashtagsInReference);
                            path = path.RemoveDouble('#'); // Try to see if it was just a typo
                        }
                        if (path.StartsWith('#'))
                        {
                            if (!localEntities.Contains(file.RelativeLowercaseName + path))
                            {
                                report.Add(reference, WarningType.UnresolvedReference);
                            }                                
                            continue;
                        }
                    }
                    if (!localEntities.Contains(CombineFilePaths(file, path)))
                    {
                        report.Add(reference, WarningType.UnresolvedReference);
                    }                        
                }
            }

            ValidateLinks(report, externalLinks);

            return report;
        }

        // This regex is expensive, therefore we use AnchorRegex first
        private static readonly Regex FragmentIdentifierRegex =
            new Regex(@"<a(?=.*href=""#(.*?)"")(?=.*(?:name|id)=""\1"").*?>", RegexOptions.Compiled);
        private static readonly Regex AnchorRegex =
            new Regex(@"<a.*?>", RegexOptions.Compiled);

        public static void ValidateLinks(Report report, List<MarkdownEntity> externalLinks)
        {
            HttpClient client = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = false });

            List<List<MarkdownEntity>> linksByBase = new List<List<MarkdownEntity>>();
            Dictionary<string, List <MarkdownEntity>> linksByBaseDict = new Dictionary<string, List<MarkdownEntity>>();
            foreach (var link in externalLinks)
            {
                if (link.Value == string.Empty ||
                    link.Value.Contains("127.0.0.1") ||
                    link.Value.Contains("localhost"))
                    report.Add(link, WarningType.Link_Invalid);
                else if (linksByBaseDict.TryGetValue(link.CleanValue, out List<MarkdownEntity> links))
                {
                    if (links.Where(l => l.Value == link.Value).Count() == 0)
                        links.Add(link);
                }
                else
                {
                    var list = new List<MarkdownEntity> { link };
                    linksByBase.Add(list);
                    linksByBaseDict.Add(link.CleanValue, list);
                }
            }

            const int MaxRedirects = 5;
            // Speed up the link checks by doing a few at once
            // If links are good it will only check one link per link-base (different fragments)
            Parallel.ForEach(linksByBase, new ParallelOptions() { MaxDegreeOfParallelism = 8 },
                list =>
                {
                    HashSet<string> fragmentIdentifiers = new HashSet<string>();

                    void testLinkForFragmentIdentifiers(MarkdownEntity link)
                    {
                        int hashtagIndex = link.Value.IndexOf('#');
                        if (hashtagIndex != -1)
                        {
                            string fragmentIdentifier = link.Value.Substring(hashtagIndex + 1).Trim().ToLower();
                            if (!fragmentIdentifiers.Contains(fragmentIdentifier))
                                report.Add(link, WarningType.Link_FragmentIdentifierNotDefined);
                        }
                    }

                    for (int i = 0; i < list.Count; i++)
                    {
                        var externalLink = list[i];
                        Queue<string> redirectQueue = new Queue<string>();

                        string constructRedirectChain()
                            => Environment.NewLine + "Redirect chain: " + string.Join(
                                    $" => {Environment.NewLine}\t\t",
                                    redirectQueue.Select(redirect => $"`{redirect}`"));

                        bool testLink(string requestUri)
                        {
                            redirectQueue.Enqueue(requestUri);
                            if (redirectQueue.Count == MaxRedirects)
                                return false;

                            Uri uri;
                            try
                            {
                                uri = new Uri(requestUri);
                            }
                            catch
                            {
                                report.Add(externalLink, WarningType.Link_Invalid);
                                return false;
                            }

                            try
                            {
                                using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
                                using (var response = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                                {
                                    int statusCode = (int)response.StatusCode;
                                    if (statusCode.IsInRange(200, 299)) // OK
                                    {
                                        if (response.Content.Headers.ContentType.MediaType == "text/html")
                                        {
                                            string pageSource = response.Content.ReadAsStringAsync().Result;
                                            foreach (Match anchor in AnchorRegex.Matches(pageSource))
                                            {
                                                Match id = FragmentIdentifierRegex.Match(anchor.Value);
                                                if (id.Success)
                                                    fragmentIdentifiers.Add(id.Groups[1].Value);

                                            }
                                        }
                                        return true;
                                    }
                                    else if (statusCode.IsInRange(300, 399)) // Redirect
                                    {
                                        Uri redirectUri = response.Headers.Location;
                                        if (!redirectUri.IsAbsoluteUri)
                                            redirectUri = new Uri(request.RequestUri.GetLeftPart(UriPartial.Authority) + redirectUri);
                                        return testLink(redirectUri.AbsoluteUri);
                                    }
                                    else // Error code
                                    {
                                        report.Add(externalLink, WarningType.Link_SiteReturnedErrorCode,
                                            detailedDescription: string.Format("Status code: {0}{1}",
                                                statusCode,
                                                redirectQueue.Count > 1
                                                    ? Environment.NewLine + constructRedirectChain()
                                                    : string.Empty));
                                        return false;
                                    }
                                }
                            }
                            catch (AggregateException ae) when (ae.InnerException is HttpRequestException)
                            {
                                report.Add(externalLink, WarningType.Link_ConnectionRefused);
                                return false;
                            }
                        }

                        bool success = testLink(externalLink.CleanValue);

                        if (redirectQueue.Count.IsInRange(2, MaxRedirects - 1))
                            report.Add(externalLink, WarningType.Link_Redirect, constructRedirectChain());
                        else if (redirectQueue.Count == MaxRedirects)
                            report.Add(externalLink, WarningType.Link_TooManyRedirects, constructRedirectChain());

                        if (success)
                        {
                            for (; i < list.Count; i++)
                            {
                                testLinkForFragmentIdentifiers(list[i]);
                            }
                        }
                    }
                });
        }

        private static List<string> GetAllFiles(string path, out List<string> subdirectories)
        {
            List<string> files = new List<string>();
            List<string> subDirs = new List<string>();

            void index(string directory)
            {
                files.AddRange(Directory.GetFiles(directory));
                subDirs.AddRange(Directory.GetDirectories(directory));
            }

            index(path);

            for (int i = 0; i < subDirs.Count; i++)
                index(subDirs[i]);

            subdirectories = subDirs;
            return files;
        }
        private static string CombineFilePaths(MarkdownFile baseFile, string relative)
        {
            if (relative.StartsWith("./", StringComparison.Ordinal))
                relative = relative.Substring(2);

            string baseDirectory = baseFile.RelativeLowercaseDirectory;
            if (baseDirectory.Length == 0) return relative;
            string relativeDirectory = Path.GetDirectoryName(relative).Replace('\\', '/');
            string relativeFilename = Path.GetFileName(relative);

            while (relativeDirectory.StartsWith("../", StringComparison.Ordinal))
            {
                relativeDirectory = relativeDirectory.Substring(3);
                if (baseDirectory.Length == 0) return "!invalid_path!";
                baseDirectory = Path.GetDirectoryName(baseDirectory).Replace('\\', '/');
            }

            return (baseDirectory.Length == 0 ? "" : baseDirectory + '/') +
                (relativeDirectory.Length == 0 ? "" : relativeDirectory + '/') + relativeFilename;
        }
    }
}
