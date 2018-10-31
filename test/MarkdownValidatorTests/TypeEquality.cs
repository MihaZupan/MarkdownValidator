/*
    Copyright (c) 2018 Miha Zupan. All rights reserved.
    This file is a part of the Markdown Validator project
    It is licensed under the Simplified BSD License (BSD 2-clause).
    For more information visit:
    https://github.com/MihaZupan/MarkdownValidator/blob/master/LICENSE
*/
using Markdig.Syntax;
using MihaZupan.MarkdownValidator.Configuration;
using MihaZupan.MarkdownValidator.Tests.Framework;
using MihaZupan.MarkdownValidator.Warnings;
using System.Linq;
using Xunit;

namespace MihaZupan.MarkdownValidator.Tests
{
    public class TypeEquality
    {
        [Fact]
        public void ValidationReport_Equals_Empty()
        {
            var validator = new MarkdownContextValidator();

            Assert.Equal(validator.Validate(), validator.ValidateFully());
        }

        [Fact]
        public void ValidationReport_Differs_WarningCount()
        {
            var reportOne = ValidationReportProvider.GetReport("Test.md", string.Empty);
            var reportTwo = ValidationReportProvider.GetReport("Test.md", "Foo");

            Assert.NotEqual(reportOne, reportTwo);
        }

        [Fact]
        public void ValidationReport_Differs_WarningDifference()
        {
            var reportOne = ValidationReportProvider.GetReport("Test.md", "[Bar](bar.txt)"); // Unresolved
            var reportTwo = ValidationReportProvider.GetReport("Test.md", "[Foo](../bar.txt)"); // OutOfContext

            Assert.NotEqual(reportOne, reportTwo);
        }

        [Fact]
        public void ValidationReport_Differs_WarningLocation()
        {
            var reportOne = ValidationReportProvider.GetReport("Test.md", "[Bar](bar.txt)");
            var reportTwo = ValidationReportProvider.GetReport("Test.md", " [Bar](bar.txt)");

            Assert.NotEqual(reportOne, reportTwo);
        }

        [Fact]
        public void Warning_Equals()
        {
            var reportOne = ValidationReportProvider.GetReport("Test.md", "[Bar](bar.txt)");
            var reportTwo = ValidationReportProvider.GetReport("Test.md", "[Foo](bar.txt)");

            var warningOne = reportOne.WarningsByFile.Single().Value.Single();
            var warningTwo = reportTwo.WarningsByFile.Single().Value.Single();

            Assert.NotSame(warningOne, warningTwo);
            Assert.Equal(warningOne, warningTwo);
        }

        [Fact]
        public void WarningID_Equals()
        {
            var warningIdOne = new WarningID(-9001, "Foo");
            var warningIdTwo = new WarningID(-9001, "Bar");

            Assert.NotSame(warningIdOne, warningIdTwo);
            Assert.Equal(warningIdOne, warningIdTwo);

            warningIdOne = WarningIDs.EmptyCodeBlock;
            warningIdTwo = new WarningID(warningIdOne.ID, string.Empty);

            Assert.NotSame(warningIdOne, warningIdTwo);
            Assert.Equal(warningIdOne, warningIdTwo);
        }

        [Fact]
        public void WarningLocation_Equals()
        {
            var warningLocationOne = new WarningLocation("Foo", "Foo", new SourceSpan(1, 2));
            var warningLocationTwo = new WarningLocation("Foo", "Foo", new SourceSpan(1, 2));

            Assert.NotSame(warningLocationOne, warningLocationTwo);
            Assert.Equal(warningLocationOne, warningLocationTwo);
        }

        [Fact]
        public void WarningLocation_Differs_FileName()
        {
            var warningLocationOne = new WarningLocation("Foo", "Foo", new SourceSpan(1, 2));
            var warningLocationTwo = new WarningLocation("Bar", "Bar", new SourceSpan(1, 2));

            Assert.NotEqual(warningLocationOne, warningLocationTwo);
        }

        [Fact]
        public void WarningLocation_Differs_Span()
        {
            var warningLocationOne = new WarningLocation("Foo", "Foo", new SourceSpan(1, 2));
            var warningLocationTwo = new WarningLocation("Foo", "Foo", new SourceSpan(5, 10));

            Assert.NotEqual(warningLocationOne, warningLocationTwo);
        }
    }
}
