using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.ContentFactory;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using Xunit;

namespace form_builder_tests.UnitTests.ContentFactory
{
    public class PageContentFactoryTests
    {
        private readonly PageContentFactory _factory;
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        
        public PageContentFactoryTests()
        {
            _factory = new PageContentFactory(_mockPageHelper.Object);
        }

        [Fact]
        public async Task Build_ShouldCallPageService_AndReturnsStringValue()
        {
            // Arrange
            var html = "testHtml";
            _mockPageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()))
                .ReturnsAsync(new form_builder.ViewModels.FormBuilderViewModel{ RawHTML = html });

            // Act
            var result = await _factory.Build(new Page(), new FormSchema(), string.Empty);

            // Assert
            Assert.Equal(html, result);
            _mockPageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>(), It.IsAny<List<OrganisationSearchResult>>()), Times.Once);
        }
    }
}
