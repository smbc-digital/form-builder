using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class CheckboxTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new ();
        private readonly Mock<IElementHelper> _mockElementHelper = new ();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new ();

        [Fact]
        public async Task RenderAsync_Should_Call_ElementHelper_ToCheck_If_Options_NeedToBeOrdered()
        {
            //Arrange
            //Arrange
            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOrderOptionsAlphabetically(true)
                .WithQuestionId("test")
                .Build();

            var viewModel = new Dictionary<string, dynamic>();

            var formAnswers = new FormAnswers();
            //Act
            await element.RenderAsync(
                _mockIViewRender.Object,
                _mockElementHelper.Object,
                "",
                viewModel,
                new Page(),
                new FormSchema(),
                _mockHostingEnv.Object,
                formAnswers);

            //Assert
            _mockElementHelper.Verify(_ => _.OrderOptionsAlphabetically(It.IsAny<Element>()), Times.Once);
        }

        [Fact]
        public void GenerateCheckboxProperties_ShouldAddDataAttribute_ForExclusiveOption() {
            //Arrange
            var options = new List<Option> {
                new Option {
                    Text = "test",
                    Value = "test",
                    Exclusive = true
                }
            };

            //Arrange
            var element = (Checkbox) new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .WithQuestionId("test")
                .Build();

            //Act
            var result = element.GenerateCheckboxProperties(0);

            //Assert
            Assert.True(result.ContainsKey("data-behaviour"));
        }
    }
}