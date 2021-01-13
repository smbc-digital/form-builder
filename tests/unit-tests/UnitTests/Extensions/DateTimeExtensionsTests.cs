using System;
using form_builder.Utils.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class DateTimeExtensionsTests
    {

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4AM()
        {
            var result = DateTime.Today.Add(new TimeSpan(4, 0, 0)).ToTimeFormat();

            Assert.Equal("4am", result);
        }

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4PM()
        {
            var result = DateTime.Today.Add(new TimeSpan(16, 0, 0)).ToTimeFormat();

            Assert.Equal("4pm", result);
        }

        [Fact]
        public void ToTimeFormat_ShouldReturn_CorrectTimeFormat_ForTime_4_30PM()
        {
            var result = DateTime.Today.Add(new TimeSpan(16, 30, 0)).ToTimeFormat();

            Assert.Equal("4:30pm", result);
        }

        [Fact]
        public void ToFullDateFormat_ShouldReturnCorrect_FullDayFormat_ForDate_Without_0()
        {
            var date = new DateTime(2020, 1, 1);
            var result = date.ToFullDateFormat();

            Assert.Equal("Wednesday 1 January 2020", result);
        }

        [Fact]
        public void ToFullDateFormat_ShouldReturnCorrect_FullDayFormat()
        {
            var date = new DateTime(2020, 1, 20);
            var result = date.ToFullDateFormat();

            Assert.Equal("Monday 20 January 2020", result);
        }

        [Fact]
        public void ToFullDateWithTimeFormat_ShouldReturnCorrect_FullDateFormat()
        {
            var date = new DateTime(2020, 1, 20).Add(new TimeSpan(1, 33, 00));
            var result = date.ToFullDateWithTimeFormat();

            Assert.Equal("Monday 20 January 2020 01:33 AM", result);
        }
    }
}
