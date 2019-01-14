/*
    Copyright (c) 2019 Miha Zupan. All rights reserved.
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
    public class GitHub
    {
        [Theory]
        [Trait("Category", "Network")]
        [InlineData("https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs")]
        [InlineData("https://github.com/TelegramBots/Telegram.Bot/blob/85134a4b5f09de4bef4306aa44d708c202b31f31/src/Telegram.Bot/Types/Message.cs")]
        [InlineData("https://github.com/TelegramBots/Telegram.Bot/commit/3bd5e058a2ed671219457127a4ac9f8e5f64ab5e#diff-4497d3d3d10a95262b32773db3da8754L317")]
        [InlineData("https://github.com/TelegramBots/Telegram.Bot/pull/802/files#diff-4695ad32fe3901a03d8952b00f316040R38")]
        [InlineData("https://github.com/MihaZupan/MarkdownValidator")]
        public void IgnoredLinks(string url)
        {
            SingleFileTest.AssertNoWarnings($"[Foo]({url})");
        }

        [Fact]
        [Trait("Category", "Network")]
        public void IgnoredAutoLink()
        {
            SingleFileTest.AssertNoWarnings("https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42");
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chat", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("*Chat*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("*_Chat_*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("`*Chat*`", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("User", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L54")]
        [InlineData("ForwardFrom", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L54")]
        [InlineData("Credentials", "https://github.com/MihaZupan/HttpToSocks5Proxy/blob/05f73139188c7e706020ec3bdaf86ce921d86ff0/src/HttpToSocks5Proxy/HttpToSocks5Proxy.cs#L20")]
        public void ValidLinks(string label, string url)
        {
            SingleFileTest.AssertNoWarnings($"[{label}]({url})");
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chatting", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L14")]
        [InlineData("*Chat*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L1")]
        [InlineData("*_Chat_*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L9001")]
        [InlineData("`*Chat*`", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L43")]
        [InlineData("ForwardTo", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L54")]
        [InlineData("Creds", "https://github.com/MihaZupan/HttpToSocks5Proxy/blob/05f73139188c7e706020ec3bdaf86ce921d86ff0/src/HttpToSocks5Proxy/HttpToSocks5Proxy.cs#L20")]
        public void InvalidLinks(string label, string url)
        {
            SingleFileTest.AssertWarning($"[{label}]({url})", WarningIDs.NoLabelOnGitHubLineUrl, 3 + label.Length, 3 + label.Length + url.Length - 1, url);
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chat", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("Credentials", "https://github.com/MihaZupan/HttpToSocks5Proxy/blob/05f73139188c7e706020ec3bdaf86ce921d86ff0/src/HttpToSocks5Proxy/HttpToSocks5Proxy.cs#L20")]
        public void ValidLinks_LinkReferenceDefinitions(string label, string url)
        {
            SingleFileTest.AssertNoWarnings($"[{label}][foo]\n\n[foo]: {url}");
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chatting", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L14")]
        [InlineData("*Chat*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L1")]
        public void InvalidLinks_LinkReferenceDefinitions(string label, string url)
        {
            SingleFileTest.AssertWarning($"[{label}][foo]\n\n[foo]: {url}", WarningIDs.NoLabelOnGitHubLineUrl, 1, label.Length, url);
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chat", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L42")]
        [InlineData("Credentials", "https://github.com/MihaZupan/HttpToSocks5Proxy/blob/05f73139188c7e706020ec3bdaf86ce921d86ff0/src/HttpToSocks5Proxy/HttpToSocks5Proxy.cs#L20")]
        public void ValidLinks_SameNameLinkReferenceDefinitions(string label, string url)
        {
            SingleFileTest.AssertNoWarnings($"[{label}]\n\n[{label}]: {url}");
        }

        [Theory]
        [Trait("Category", "Network")]
        [InlineData("Chatting", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L14")]
        [InlineData("*Chat*", "https://github.com/TelegramBots/Telegram.Bot/blob/master/src/Telegram.Bot/Types/Message.cs#L1")]
        public void InvalidLinks_SameNameLinkReferenceDefinitions(string label, string url)
        {
            SingleFileTest.AssertWarning($"[{label}]\n\n[{label}]: {url}", WarningIDs.NoLabelOnGitHubLineUrl, 1, label.Length, url);
        }
    }
}
