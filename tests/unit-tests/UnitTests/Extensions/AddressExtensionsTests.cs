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

        [Fact]
        public void RemoveTagParsing_Should_RemoveBrackets_FromQuestionId()
        {
            var address = "{{QUESTION:addressReference}}";
            var result = address.RemoveTagParsingFromQuestionId();

            Assert.Equal("addressReference", result);
        }

        [Fact]
        [InlineData("1 London Road, Manchester, M12 4DR")]
        public void ConvertStringToObject_Should_ReturnHouseNo_IfBeginsWithNumber()
        {
            var address = "1 London Road, Manchester, M12 4DR";
            var result = address.ConvertStringToObject();

            Assert.Equal("1", result.HouseNo);
            Assert.Equal("London Road", result.Street);
            Assert.Equal("Manchester", result.Town);
            Assert.Equal("M12 4DR", result.Postcode);
        }

        [Fact]
        public void ConvertStringToObject_Should_ReturnStreet_IfBeginsWithWord()
        {
            var address = "Flat 10 Boddingtons London Road, Manchester, M12 4DR";
            var result = address.ConvertStringToObject();

            Assert.Equal("Flat 10 Boddingtons London Road", result.Street);
            Assert.Equal("Manchester", result.Town);
            Assert.Equal("M12 4DR", result.Postcode);
            Assert.Null(result.HouseNo);
        }
    }
}