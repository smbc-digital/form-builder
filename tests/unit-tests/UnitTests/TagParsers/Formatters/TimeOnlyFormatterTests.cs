using System;
using form_builder.TagParser;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class TimeOnlyFormatterTests
    {
        private TimeOnlyFormatter _formatter = new TimeOnlyFormatter();

        [Fact]
        public void Parse_ShouldReturn_CorrectTimeFormat()
        {
            Assert.Equal("3:30pm", _formatter.Parse(DateTime.Today.Add(new TimeSpan(15, 30, 0)).ToString()));
        }

    }
}