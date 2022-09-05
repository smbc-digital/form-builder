using form_builder.Constants;
using form_builder.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class DictionaryExtensionsTests
    {
        [Theory]
        [InlineData("<script>window.alert('HACKED!')</script>")]
        [InlineData("first-name</><p onload=hack()></p>")]
        public void ToNormaliseDictionary_Should_EncodeAngleBrackets(string userInput)
        {
            // Arrange
            string questionId = "questionId";
            Dictionary<string, string[]> viewModel = new() { { questionId, new string[] { userInput } } };

            // Act
            var result = viewModel.ToNormaliseDictionary(string.Empty);
            result.TryGetValue(questionId, out var encodedUserInput);

            // Assert
            Assert.True(userInput.Contains("<") || userInput.Contains(">"));
            Assert.True(!encodedUserInput.ToString().Contains("<") || !encodedUserInput.ToString().Contains(">"));
        }

        [Fact]
        public void ToNormaliseDictionary_ShouldRemoveAny_Keys_AssociatedWith_Time_And_Add_AppointmentStart_EndTime()
        {
            var questionId = "bookingTestQuestion";
            var selectedStartTime = new DateTime(2020, 12, 15, 15, 0, 0).ToString();
            var selectedEndTime = new DateTime(2020, 12, 15, 16, 0, 0).ToString();

            var viewModel = new Dictionary<string, string[]>
            {
                { $"13{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}", new string[1] { BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING } },
                { $"15{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}", new string[1] { BookingConstants.APPOINTMENT_TIME_OF_DAY_AFTERNOON } },
                { $"20{BookingConstants.APPOINTMENT_TIME_OF_DAY_SUFFIX}", new string[1] { BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING } },
                { $"{questionId}-{BookingConstants.APPOINTMENT_DATE}", new string[1] { new DateTime(2020, 12, 15).ToString() } },
                { $"{questionId}-{BookingConstants.APPOINTMENT_START_TIME}-13-{BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING}", new string[1] { $"{new DateTime(2020, 12, 13, 7, 30, 0).ToString()}|{new DateTime(2020, 12, 13, 10, 0, 0).ToString()}" } },
                { $"{questionId}-{BookingConstants.APPOINTMENT_START_TIME}-13-{BookingConstants.APPOINTMENT_TIME_OF_DAY_AFTERNOON}", new string[1] { $"{new DateTime(2020, 12, 13, 13, 30, 0).ToString()}|{new DateTime(2020, 12, 13, 14, 0, 0).ToString()}" } },
                { $"{questionId}-{BookingConstants.APPOINTMENT_START_TIME}-15-{BookingConstants.APPOINTMENT_TIME_OF_DAY_MORNING}", new string[1] { $"{new DateTime(2020, 12, 15, 9, 30, 0).ToString()}|{new DateTime(2020, 12, 15, 10, 0, 0).ToString()}" } },
                { $"{questionId}-{BookingConstants.APPOINTMENT_START_TIME}-15-{BookingConstants.APPOINTMENT_TIME_OF_DAY_AFTERNOON}", new string[1] { $"{selectedStartTime}|{selectedEndTime}" } },
            };

            var result = viewModel.ToNormaliseDictionary(string.Empty);

            Assert.Equal(4, result.Count);
            Assert.True(result.ContainsKey($"{questionId}-{BookingConstants.APPOINTMENT_START_TIME}"));
            Assert.True(result.ContainsKey($"{questionId}-{BookingConstants.APPOINTMENT_END_TIME}"));
            Assert.True(result.ContainsValue(selectedStartTime));
            Assert.True(result.ContainsValue(selectedEndTime));
        }


        [Fact]
        public void ToNormaliseDictionary_Should_Not_ChangeDictionary_When_ViewModel_Does_NotContain_Time_Of_Day_Key()
        {
            var viewModel = new Dictionary<string, string[]>
            {
                { $"questionone", new string[1] { "test" } },
                { $"questiontwo", new string[1] { "data" } }
            };

            var result = viewModel.ToNormaliseDictionary(string.Empty);

            Assert.Equal(3, result.Count);
        }
    }
}
