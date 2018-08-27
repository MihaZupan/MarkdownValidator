namespace ReferenceValidator.Parser
{
    enum MarkdownEntityType
    {
        /// <summary>
        /// `Name` in:
        /// <para>[Name]: "http.."</para>
        /// </summary>
        DefinedNamedReference,

        /// <summary>
        /// `Name` in:
        /// <para>[Test][Name]</para>
        /// </summary>
        AccessedNamedReference,

        /// <summary>
        /// `Name` in:
        /// <para>Random [Name]</para>
        /// </summary>
        AccessedSameNameReference,

        /// <summary>
        /// `Header name` in:
        /// <para># Header name</para>
        /// </summary>
        Header,

        /// <summary>
        /// Reference to a local entity (with or without a fragment identifier)
        /// </summary>
        Reference,

        /// <summary>
        /// Full http://... link
        /// </summary>
        ExternalLink,

        /// <summary>
        /// ``, `a`, `b` in:
        /// <para>[] [a][b]</para>
        /// </summary>
        EmptySquareBracketsEntity,

        /// <summary>
        /// e.g. details>
        /// </summary>
        BrokenHtmlTag,

        UnmatchedHtmlTags,
    }
}
