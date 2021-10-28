using System;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class EnumExtensionsTests
    {
        [Fact]
        public void ToReadableTextForAnlayticsEvent_ShouldReturn_Readable_AnalyticsEvent_From_Enum()
        {
            // Act
            var result = EAnalyticsEventType.Finish.ToReadableTextForAnlayticsEvent();

            // Assert
            Assert.Equal(AnalyticsConstants.FINISH_EVENT_LABEL_NAME, result);
        }

        [Fact]
        public void ToReadableTextForAnlayticsEvent_Should_Throw_Exception_When_Unknown_Enum_Type()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => EAnalyticsEventType.Unknown.ToReadableTextForAnlayticsEvent());
        }
    }
}
