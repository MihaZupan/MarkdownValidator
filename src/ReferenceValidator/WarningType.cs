namespace ReferenceValidator
{
    enum WarningType
    {
        InternalError,

        MultipleSpacesInHeader,
        UnusedNamedReference,
        UndefinedNamedReference,
        EmptyFragmentIdentifier,
        MultipleHashtagsInReference,
        UnresolvedReference,
        EmptySquareBrackets,
        PossibleUnresolvedReference,

        Link_FragmentIdentifierNotDefined,
        Link_SiteReturnedErrorCode,
        Link_Redirect,
        Link_TooManyRedirects,
        Link_Invalid,
        Link_ConnectionRefused,

        Html_BrokenTag,
        Html_UnmatchedTag
    }
}
