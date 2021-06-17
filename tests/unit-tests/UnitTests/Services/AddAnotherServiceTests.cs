using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Builders;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Services.AddAnotherService;
using form_builder.Validators;
using form_builder.ViewModels;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class AddAnotherServiceTests
    {
        private readonly AddAnotherService _addAnotherService;
        private readonly Mock<IPageHelper> _mockPageHelper = new();
        private readonly Mock<IPageFactory> _mockPageFactory = new();
        private readonly Mock<IElementValidator> _testValidator = new();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();

        public AddAnotherServiceTests()
        {
            var formAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object>
                {
                    { "addAnotherFieldset-person", 1 }
                }
            };

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(formAnswers);

            _mockPageFactory
                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
                .ReturnsAsync(new FormBuilderViewModel());

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = true });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };

            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());

            _addAnotherService = new AddAnotherService(_mockPageHelper.Object, _mockPageFactory.Object);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldThrow_If_AddEmptyFieldsetTrue_And_CurrentIncrementIsSameOrGreaterThanMaximumFieldsets()
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object>
                {
                    { "addAnotherFieldset-person", 10 }
                }
            };

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(formAnswers);

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "addAnotherFieldset", "addAnother"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            // Act
            var result = await Assert.ThrowsAsync<ApplicationException>(() => _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one"));

            // Assert
            Assert.Equal("AddAnotherService::ProcessAddAnother, maximum number of fieldsets exceeded", result.Message);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldIncreaseIncrement_And_SaveFormDataIncrement_WhenViewModelIsValid_And_AddEmptyFieldsetTrue()
        {
            // Arrange
            var formAnswers = new FormAnswers
            {
                FormData = new Dictionary<string, object>
                {
                    { "addAnotherFieldset-person", 0 }
                }
            };

            _mockPageHelper
                .Setup(_ => _.GetSavedAnswers(It.IsAny<string>()))
                .Returns(formAnswers);

            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "addAnotherFieldset", "addAnother"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> {textboxElement};

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            // Act
            await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one");

            // Assert
            _mockPageHelper.Verify(_ => _.SaveFormData("addAnotherFieldset-person", 1, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldDecreaseIncrement_And_SaveFormDataIncrement_WhenViewModelIsValid_And_AddRemovingFieldset()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "question:1:", null
                },
                {
                    "remove-0", "remove"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement> { textboxElement };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            // Act
            await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one");

            // Assert
            _mockPageHelper.Verify(_ => _.SaveFormData("addAnotherFieldset-person", 0, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldCallPageHelper_ToRemoveFieldset()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "question:1:", "answer"
                },
                {
                    "remove-0", "remove"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement>
            {
                textboxElement
            };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            // Act
            await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one");

            // Assert
            _mockPageHelper.Verify(_ => _.RemoveFieldset(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldReturnCorrectProcessRequestEntity_WhenRemovingFieldset()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "question:1:", "answer"
                },
                {
                    "remove-0", "remove"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement>
            {
                textboxElement
            };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .WithValidatedModel(true)
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            // Act
            var result = await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one");

            // Assert
            Assert.True(result.RedirectToAction);
            Assert.Equal("Index", result.RedirectAction);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldCallPageFactory_IfPageInvalid()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "question:1:", "answer"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .WithQuestionId("person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement>
            {
                textboxElement
            };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());
            page.Validate(new Dictionary<string, dynamic>(), _validators.Object, new FormSchema());

            // Act
            await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema, Guid.NewGuid().ToString(), "page-one");

            // Assert
            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAddAnother_ShouldReturnCorrectProcessRequestEntity_IfPageInvalid()
        {
            // Arrange
            var viewModel = new Dictionary<string, dynamic>
            {
                {
                    "question:0:", "answer"
                },
                {
                    "question:1:", "answer"
                }
            };

            var addAnotherElement = new ElementBuilder()
                .WithType(EElementType.AddAnother)
                .WithLabel("Person")
                .Build();

            var textboxElement = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithLabel("Name")
                .WithQuestionId("question")
                .Build();

            addAnotherElement.Properties.Elements = new List<IElement>
            {
                textboxElement
            };

            var page = new PageBuilder()
                .WithElement(addAnotherElement)
                .WithValidatedModel(true)
                .WithPageSlug("page-one")
                .Build();

            var baseSchema = new FormSchemaBuilder()
                .WithPage(page)
                .WithBaseUrl("form")
                .Build();

            _testValidator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
                .Returns(new ValidationResult { IsValid = false });

            var elementValidatorItems = new List<IElementValidator> { _testValidator.Object };
            _validators.Setup(m => m.GetEnumerator()).Returns(() => elementValidatorItems.GetEnumerator());
            page.Validate(new Dictionary<string, dynamic>(), _validators.Object, new FormSchema());

            // Act
            var result = await _addAnotherService.ProcessAddAnother(viewModel, page, baseSchema,
                Guid.NewGuid().ToString(), "page-one");

            // Assert
            Assert.False(result.RedirectToAction);
            Assert.NotNull(result.Page);
            Assert.NotNull(result.ViewModel);
        }
    }
}
