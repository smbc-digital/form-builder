using System;
using form_builder.TagParsers.Formatters;
using Xunit;

namespace form_builder_tests.UnitTests.TagParsers.Formatters
{
    public class TimeOnlyFormatterTests
    {
        private readonly TimeOnlyFormatter _formatter = new();

        [Fact]
        public void Parse_ShouldReturn_CorrectTimeFormat()
        {
            Assert.Equal("3:30pm", _formatter.Parse(DateTime.Today.Add(new TimeSpan(15, 30, 0)).ToString()));
        }

    }
}