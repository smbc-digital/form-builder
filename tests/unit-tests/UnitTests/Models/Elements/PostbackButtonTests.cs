﻿using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Models.Elements
{
    public class PostbackButtonTests
    {
        private readonly Mock<IViewRender> _mockIViewRender = new();
        private readonly Mock<IElementHelper> _mockElementHelper = new();
        private readonly Mock<IWebHostEnvironment> _mockHostingEnv = new();

        [Fact]
        public async Task RenderAsync_ShouldReturnExpectedString()
        {
            //Arrange
            var expectedResult = "<button formmethod='post' data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='addAnother' id='addAnother' 'aria-describedby=addAnother' data-module='govuk-button'> Add another </button>";
            var element = new ElementBuilder()
                .WithType(EElementType.PostbackButton)
                .WithLabel("Add another")
                .WithName("addAnother")
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
            var result = await element.RenderAsync(_mockIViewRender.Object, _mockElementHelper.Object, string.Empty, viewModel, page, schema, _mockHostingEnv.Object, formAnswers);

            //Assert
            Assert.Equal(expectedResult, result);
        }
    }
}
