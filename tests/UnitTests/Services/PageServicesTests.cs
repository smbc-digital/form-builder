using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.SchemaProvider;
using form_builder.Services.AddressService;
using form_builder.Services.PageService;
using form_builder.Services.StreetService;
using form_builder.Validators;
using form_builder_tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class PageServicesTests
    {
        private readonly PageService _service;
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<IElementValidator> _validator = new Mock<IElementValidator>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        private readonly Mock<ISessionHelper> _sessionHelper = new Mock<ISessionHelper>();
        private readonly Mock<ILogger<PageService>> _logger = new Mock<ILogger<PageService>>();
        private readonly Mock<IStreetService> _streetService = new Mock<IStreetService>();
        private readonly Mock<IAddressService> _addressService = new Mock<IAddressService>();

        public PageServicesTests()
        {
            _validator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, string>>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _validator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _service = new PageService(_logger.Object, _validators.Object, _schemaProvider.Object, _pageHelper.Object, _sessionHelper.Object, _addressService.Object, _streetService.Object);
        }

        [Fact]
        public async Task Index_Post_ApplicationShould_ThrowApplicationException_WhenGenerateHtml_ThrowsException()
        {
            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()))
                .Throws<ApplicationException>();

            var element = new ElementBuilder()
               .WithType(EElementType.Textarea)
               .WithQuestionId("test-question")
               .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var schema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(schema);

            var viewModel = new Dictionary<string, string>
            {
                { "Guid", Guid.NewGuid().ToString() },
                { "AddressStatus", "Search" },
                { $"{element.Properties.QuestionId}-postcode", "SK11aa" },
            };

            var result = await Assert.ThrowsAsync<ApplicationException>(() => _service.ProcessPage("form", "page-one", viewModel, false));

            _pageHelper.Verify(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<List<AddressSearchResult>>()), Times.Once);
        }
    }
}
