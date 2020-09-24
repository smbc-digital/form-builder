using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using form_builder.Validators;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Validators
{
    public class RestrictCombinedFileSizeValidatorTests
    {
        private readonly RestrictCombinedFileSizeValidator _validator;
        private readonly Mock<ISessionHelper> _mockSessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<IDistributedCacheWrapper> _mockDistributedCacheWrapper = new Mock<IDistributedCacheWrapper>();

        public RestrictCombinedFileSizeValidatorTests()
        {
            _validator = new RestrictCombinedFileSizeValidator(_mockSessionHelper.Object, _mockDistributedCacheWrapper.Object);
        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenNot_MultipleFileUpload_ElementType()
        {

        }
        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenQuestion_NowWithinViewModel()
        {

        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenValueIn_ViewModel_IsNull()
        {

        }

        [Fact]
        public void Validate_ShouldReturn_True_ValidationResult_WhenTotalFiles_AreBelow_MaxLimit()
        {

        }

        [Fact]
        public void Validate_ShouldReturn_False_ValidationResult_WhenTotalFiles_AreOver_MaxLimit()
        {

        }
    }
}
