using form_builder.Extensions;
using Xunit;

namespace form_builder_tests.UnitTests.Extensions
{
    public class AddressExtensionsTests
    {
        [Theory]
        [InlineData("1 London RD, MAnChester, M12 4dr", "1 London Rd, Manchester, M12 4DR")]
        [InlineData("1 LONDON RD, MANCHESTER, M12 4DR", "1 London Rd, Manchester, M12 4DR")]
        [InlineData("1 london rd, manchester, m12 4dr", "1 London Rd, Manchester, M12 4DR")]
        public void AppointmentLocation_ShouldReturnTitleCaseString_WhenLocationFound(string address, string expectedResult)
        {
            var result = address.ConvertAddressToTitleCase();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("{{QUESTION:addressReference}}", "addressReference")]
        [InlineData("{{QUESTION:addressFromQuestionId}}", "addressFromQuestionId")]
        public void RemoveTagParsing_Should_RemoveBrackets_FromQuestionId(string address, string expectedResult)
        {
            var result = address.RemoveTagParsingFromQuestionId();

            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [InlineData("1 London Road, Manchester, M12 4DR")]
        public void ConvertStringToObject_Should_ReturnHouseNo_IfBeginsWithNumber(string address)
        {
            var result = address.ConvertStringToObject();

            Assert.Equal(result.HouseNo, "1");
            Assert.Equal(result.Street, "London Road");
            Assert.Equal(result.Town, "Manchester");
            Assert.Equal(result.Postcode, "M12 4DR");
        }

        [Theory]
        [InlineData("Flat 10 Boddingtons London Road, Manchester, M12 4DR")]
        public void ConvertStringToObject_Should_ReturnStreet_IfBeginsWithWord(string address)
        {
            var result = address.ConvertStringToObject();

            Assert.Equal(result.Street, "Flat 10 Boddingtons London Road");
            Assert.Equal(result.Town, "Manchester");
            Assert.Equal(result.Postcode, "M12 4DR");
            Assert.True(result.HouseNo is null);
        }
    }
}