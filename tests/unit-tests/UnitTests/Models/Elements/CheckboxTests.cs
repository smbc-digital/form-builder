using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
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
        public async Task RenderAsync_Should_OrderOptions_When_OrderOptionsAlphabetically_Is_True()
        {
            //Arrange
            Checkbox callback = new ();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Checkbox>(), null))
                .Callback<string, Checkbox, Dictionary<string, object>>((x, y, z) => callback = y);

            var options = new List<Option> {
                new Option {
                    Text = "X",
                    Value = "X"
                },
                new Option {
                    Text = "A",
                    Value = "A"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
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
            Assert.Equal("A",callback.Properties.Options[0].Text);
            Assert.Equal("X",callback.Properties.Options[1].Text);
        }
      
        [Fact]
        public async Task RenderAsync_Should_Not_OrderOptions_When_OrderOptionsAlphabetically_Is_False()
        {
              //Arrange
            Checkbox callback = new ();
            _mockIViewRender.Setup(_ => _.RenderAsync(It.IsAny<string>(), It.IsAny<Checkbox>(), null))
                .Callback<string, Checkbox, Dictionary<string, object>>((x, y, z) => callback = y);

            var options = new List<Option> {
                new Option {
                    Text = "X",
                    Value = "X"
                },
                new Option {
                    Text = "A",
                    Value = "A"
                }
            };

            var element = new ElementBuilder()
                .WithType(EElementType.Checkbox)
                .WithOptions(options)
                .WithOrderOptionsAlphabetically(false)
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
            Assert.Equal("X",callback.Properties.Options[0].Text);
            Assert.Equal("A",callback.Properties.Options[1].Text);
        }
    }
}