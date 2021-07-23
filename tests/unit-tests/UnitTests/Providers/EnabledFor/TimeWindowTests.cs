using System;
using form_builder.Models;
using form_builder.Models.Properties.EnabledForProperties;
using form_builder.Providers.EnabledFor;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.EnabledFor
{
    public class TimeWindowTests
    {
        private readonly TimeWindow _timeWindow = new TimeWindow();

        [Fact]
        public void IsAvailable_ShouldReturnTrue_WhenDate_IsBetween_Start_And_End_Date()
        {
            var model = new EnabledForBase
            {
                Properties = new EnabledForProperties 
                {
                    Start = DateTime.Now.AddDays(-1),
                    End = DateTime.Now.AddDays(+1)
                }
            };

            Assert.True(_timeWindow.IsAvailable(model));
        }

        [Fact]
        public void IsAvailable_ShouldReturnFalse_WhenDate_IsAfter_EndDate()
        {
            var model = new EnabledForBase
            {
                Properties = new EnabledForProperties 
                {
                    Start = DateTime.Now.AddDays(-2),
                    End = DateTime.Now.AddDays(-1)
                }
            };
            
            Assert.False(_timeWindow.IsAvailable(model));
        }

        [Fact]
        public void IsAvailable_ShouldReturnFalse_WhenDate_IsBefore_StartDate()
        {
            var model = new EnabledForBase
            {
                Properties = new EnabledForProperties 
                {
                    Start = DateTime.Now.AddDays(+1),
                    End = DateTime.Now.AddDays(+2)
                }
            };
            
            Assert.False(_timeWindow.IsAvailable(model));
        }
    }
}
