using form_builder.Configuration;
using form_builder.Utils.Hash;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Utils.Hash
{
    public class HashUtilTests
    {
        private readonly HashUtil _hashUtil;
        private readonly Mock<IOptions<HashConfiguration>> _mockHashConfiguration = new ();

        private const string TEST_SALTED_HASH = "acc1bb7b60a1cdcf4032c85bf779b69ccb846825447baeb9dd644a60eb5c5ca9";

        public HashUtilTests()
        {
            _mockHashConfiguration
                .Setup(_ => _.Value)
                .Returns(new HashConfiguration
                {
                    Salt = "3267ad73-8388-499f-a082-b1551260d8fa"
                });

            _hashUtil = new HashUtil(_mockHashConfiguration.Object);
        }

        [Fact]
        public void Hash_ShouldReturnCorrectHashResult()
        {
            // Act
            string result = _hashUtil.Hash("test");

            // Assert
            Assert.Equal(TEST_SALTED_HASH, result);
        }

        [Theory]
        [InlineData("test", TEST_SALTED_HASH, true)]
        [InlineData("test", "manipulatedHash", false)]
        public void Check_ShouldValidateHash(string reference, string hash, bool expectedValidation)
        {
            // Act
            bool result = _hashUtil.Check(reference, hash);

            // Assert
            Assert.Equal(expectedValidation, result);
        }
    }
}
