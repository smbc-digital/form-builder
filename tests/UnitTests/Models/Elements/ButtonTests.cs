using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class ButtonTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IHostingEnvironment> _mockHostingEnv = new Mock<IHostingEnvironment>();
        
        [Fact]
        public async Task RenderAsync_ShouldUseAddressSearchText_ForButton_WhenAddressSearch()
        {
            var callback = new Dictionary<string, dynamic>();
            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a,b,c) => callback = c);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .Build();

            var addressEleement = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithStreetProvider("CRM")
                .Build();

            var page = new PageBuilder()
                .WithElement(addressEleement)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "",viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal(SystemConstants.AddressSearchButtonText, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Button"), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultButtonText()
        {
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .Build();

            var textBoxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .Build();

            var page = new PageBuilder()
                .WithElement(textBoxElement)
                .WithElement(element)
                .Build();


            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal(SystemConstants.SubmitButtonText, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Button"), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultSubmitButtonText_WhenSubmitAndPayBehaviour()
        {
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitAndPay)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", new List<AddressSearchResult>(), new List<OrganisationSearchResult>(), viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal(SystemConstants.SubmitButtonText, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Button"), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUse_SuppliedButtonText_DefaultButtonText()
        {
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithPropertyText("test text")
                .Build();

            var textAreaEleement = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .Build();

            var page = new PageBuilder()
                .WithElement(textAreaEleement)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, "", viewModel, page, schema, _mockHostingEnv.Object);

            //Assert
            Assert.Equal("test text", callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Button"), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}