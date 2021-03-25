using System;
using form_builder.Utils.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class TimeSpanExtensionsTest
    {

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4AM()
        {
            var result = new TimeSpan(4, 0, 0).ToTimeFormat();

            Assert.Equal("4am", result);
        }

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4PM()
        {
            var result = new TimeSpan(16, 0, 0).ToTimeFormat();

            Assert.Equal("4pm", result);
        }

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4_30PM()
        {
            var result = new TimeSpan(16, 30, 0).ToTimeFormat();

            Assert.Equal("4:30pm", result);
        }
    }
}
