using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class ButtonTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new Mock<IViewRender>();
        private readonly Mock<IElementHelper> _mockElementHelper = new Mock<IElementHelper>();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new Mock<IWebHostEnvironment>();

        [Fact]
        public async Task RenderAsync_ShouldUseAddressSearchText_ForButton_WhenAddressSearch()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a,b,c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .Build();

            var addressElement = new ElementBuilder()
                .WithType(EElementType.Address)
                .WithStreetProvider("CRM")
                .Build();

            var page = new PageBuilder()
                .WithElement(addressElement)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema,  _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(ButtonConstants.ADDRESS_SEARCH_TEXT, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultButtonText()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender
                .Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

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

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(ButtonConstants.NEXTSTEP_TEXT, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x == "Button"), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultSubmitButtonText_WhenSubmitFormBehaviour()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.SubmitForm)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(ButtonConstants.SUBMIT_TEXT, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUseDefaultSubmitButtonText_WhenSubmitAndPayBehaviour()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

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

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(ButtonConstants.SUBMIT_TEXT, callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_ShouldUse_SuppliedButtonText_DefaultButtonText()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithPropertyText("test text")
                .Build();

            var textAreaElement = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .Build();

            var page = new PageBuilder()
                .WithElement(textAreaElement)
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal("test text", callback.Properties.Text);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Fact]
        public async Task RenderAsync_DisableButton_OnClick_ShouldDefault_ToFalse()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithPropertyText("test text")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.False(callback.Properties.DisableOnClick);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
        
        [Fact]
        public async Task RenderAsync_ShouldDisableButton_OnClick_WhenPropertyIsEnabled()
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithPropertyText("test text")
                .WithDisableOnClick(true)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.True(callback.Properties.DisableOnClick);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }

        [Theory]
        [InlineData(EBehaviourType.SubmitAndPay)]
        [InlineData(EBehaviourType.SubmitForm)]
        public async Task RenderAsync_ShouldDisableButton_OnClick_WhenPageHas_SubmitForm_OrSubmitAndPay_Action(EBehaviourType type)
        {
            //Arrange
            var callback = new Button();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Button>(), It.IsAny<Dictionary<string, dynamic>>()))
                    .Callback<string, Button, Dictionary<string, dynamic>>((a, b, c) => callback = b);

            var element = new ElementBuilder()
                .WithType(EElementType.Button)
                .WithPropertyText("test text")
                .Build();

            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(type)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithBehaviour(behaviour)
                .Build();

            var schema = new FormSchemaBuilder()
                .WithName("form-name")
                .Build();

            var formAnswers = new FormAnswers();

            var viewModel = new Dictionary<string, dynamic>();

            //Act
            await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.True(callback.Properties.DisableOnClick);
            _mockIViewRender.Verify(_ => _.RenderAsync(It.Is<string>(x => x.Equals("Button")), It.IsAny<Button>(), It.IsAny<Dictionary<string, object>>()), Times.Once);
        }
    }
}