/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using MihaZupan.MarkdownValidator.Tests.Framework;
using MihaZupan.MarkdownValidator.Warnings;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests.ExternalUrlsTests
{
    public class WebRequestErrorCode
    {
        [Theory]
        [InlineData(Constants.TEST_HOST + "/status/100")]
        [InlineData(Constants.TEST_HOST + "/status/400")]
        [InlineData(Constants.TEST_HOST + "/status/500")]
        [InlineData(Constants.TEST_HOST + "/status/504")]
        public void ErrorCode(string url)
        {
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestReturnedErrorCode, 6, 6 + url.Length - 1, url);
        }

        private const string RedirectUrl = Constants.TEST_HOST + "/redirect-to?url=" + Constants.TEST_HOST_ENCODED + "%2Fstatus%2F";
        [Theory]
        [InlineData(RedirectUrl + "100")]
        [InlineData(RedirectUrl + "400")]
        [InlineData(RedirectUrl + "500")]
        [InlineData(RedirectUrl + "504")]
        public void ErrorCodeAfterRedirect(string url)
        {
            string source = $"[Foo]({url})";
            SingleFileTest.AssertWarning(source,
                WarningIDs.WebRequestReturnedErrorCode, 6, 6 + url.Length - 1, url);
        }
    }
}
