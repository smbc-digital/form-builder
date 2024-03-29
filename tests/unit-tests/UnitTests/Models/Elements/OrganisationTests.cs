using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Gateways.Models.Verint.Lookup;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class OrganisationTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        public OrganisationTests()
        {
            _mockHostingEnv.Setup(_ => _.EnvironmentName).Returns("local");
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithListOf_SelectListItem_FromOrgSearchResults()
        {
            //Arrange
            var callback = new form_builder.Models.Elements.Organisation();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, form_builder.Models.Elements.Organisation, Dictionary<string, object>>((x, y, z) => callback = y);

            var organisationElement = (form_builder.Models.Elements.Organisation)new ElementBuilder()
                .WithType(EElementType.Organisation)
                .WithStreetProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(organisationElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic },
                { $"{organisationElement.Properties.QuestionId}-organisation", "test org" }
            };

            //Act
            await organisationElement.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "OrganisationSelect"), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Single(callback.Items);
        }

        [Fact]
        public async Task RenderAsync_ShouldCall_ViewRender_WithListOf_SelectListItem_GeneratedFromOrgSearchResults()
        {
            //Arrange
            var callback = new form_builder.Models.Elements.Organisation();

            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()))
                .Callback<string, form_builder.Models.Elements.Organisation, Dictionary<string, object>>((x, y, z) => callback = y);

            var organisationElement = (form_builder.Models.Elements.Organisation)new ElementBuilder()
                .WithType(EElementType.Organisation)
                .WithStreetProvider("Fake")
                .Build();

            var page = new PageBuilder()
                .WithElement(organisationElement)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>
            {
                { LookUpConstants.SubPathViewModelKey, LookUpConstants.Automatic },
                { $"{organisationElement.Properties.QuestionId}-organisation", "test org" }
            };

            var searchResults = new List<object>
            {
                new OrganisationSearchResult{ Reference = "123455", Name = "name123" }
            };

            //Act
            await organisationElement.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers, searchResults);

            //Assert
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "OrganisationSelect"), It.IsAny<form_builder.Models.Elements.Organisation>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
            Assert.Equal(2, callback.Items.Count);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_IsDefined()
        {
            var label = "Custom label";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithQuestionId("org-test")
                .WithSummaryLabel(label)
                .Build();

            //Act
            var result = element.GetLabelText(string.Empty);

            //Assert
            Assert.Equal(label, result);
        }

        [Fact]
        public void GetLabelText_ShouldGenerate_CorrectLabel_WhenSummaryLabel_Is_Not_Defined()
        {
            var pageTitle = "Page Title";
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Street)
                .WithQuestionId("org-test")
                .Build();

            //Act
            var result = element.GetLabelText(pageTitle);

            //Assert
            Assert.Equal(pageTitle, result);
        }
    }
}