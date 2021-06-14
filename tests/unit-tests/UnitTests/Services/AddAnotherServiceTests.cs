//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using form_builder.Builders;
//using form_builder.ContentFactory.PageFactory;
//using form_builder.Enum;
//using form_builder.Factories.Transform.AddAnother;
//using form_builder.Helpers.PageHelpers;
//using form_builder.Models;
//using form_builder.Models.Elements;
//using form_builder.Services.AddAnotherService;
//using form_builder.Validators;
//using form_builder.ViewModels;
//using form_builder_tests.Builders;
//using Moq;
//using Xunit;

//namespace form_builder_tests.UnitTests.Services
//{
//    public class AddAnotherServiceTests
//    {
//        private readonly AddAnotherService _addAnotherService;
//        private readonly Mock<IPageHelper> _mockPageHelper = new();
//        private readonly Mock<IPageFactory> _mockPageFactory = new();
//        private readonly Mock<IUserPageTransformFactory> _mockAddAnotherSchemaTransformFactory = new();
//        private readonly AddAnotherSchemaTransformFactory _addAnotherSchemaTransformFactory = new();
//        private readonly Mock<IEnumerable<IElementValidator>> _validators = new();
//        private readonly Mock<IElementValidator> _validator = new();

//        public AddAnotherServiceTests()
//        {
//            _mockPageFactory
//                .Setup(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()))
//                .ReturnsAsync(new FormBuilderViewModel());

//            _addAnotherService = new AddAnotherService(_mockPageHelper.Object, _mockPageFactory.Object, _mockAddAnotherSchemaTransformFactory.Object);

//            _validator.Setup(_ => _.Validate(It.IsAny<Element>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>()))
//                .Returns(new ValidationResult { IsValid = false });

//            var elementValidatorItems = new List<IElementValidator> { _validator.Object };
//            _validators
//                .Setup(m => m.GetEnumerator())
//                .Returns(() => elementValidatorItems.GetEnumerator());
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldUpdateAndSaveSchema_WhenViewModelIsValid_And_AddEmptyFieldsetTrue()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "addAnotherFieldset", "addAnother"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>();
//            addAnotherElement.Properties.Elements.Add(textboxElement);

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .WithValidatedModel(true)
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];

//            // Act
//            await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            _mockAddAnotherSchemaTransformFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Once());
//            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldUpdateAndSaveSchema_WhenViewModelIsValid_And_AddRemovingFieldset()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                },
//                {
//                    "remove-0", "remove"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .WithValidatedModel(true)
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];

//            // Act
//            await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            _mockAddAnotherSchemaTransformFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Once());
//            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldCallPageHelper_ToRemoveFieldset()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                },
//                {
//                    "remove-0", "remove"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .WithValidatedModel(true)
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];

//            // Act
//            await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            _mockPageHelper.Verify(_ => _.RemoveFieldset(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldReturnCorrectProcessRequestEntity_WhenRemovingFieldset()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                },
//                {
//                    "remove-0", "remove"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .WithValidatedModel(true)
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];

//            // Act
//            var result = await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            Assert.True(result.RedirectToAction);
//            Assert.Equal("Index", result.RedirectAction);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldNotCallPageHelperOrTransformSchema_IfFieldsetNotBeingAddedOrRemoved()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .WithValidatedModel(true)
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];

//            // Act
//            await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            _mockPageHelper.Verify(_ => _.RemoveFieldset(It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//            _mockAddAnotherSchemaTransformFactory.Verify(_ => _.Transform(It.IsAny<FormSchema>()), Times.Never());
//            _mockPageHelper.Verify(_ => _.SaveFormData(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldCallPageFactory_IfPageInvalid()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];
//            dynamicFormPage.Validate(new Dictionary<string, dynamic>(), _validators.Object, baseSchema);

//            // Act
//            await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema, Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            _mockPageFactory.Verify(_ => _.Build(It.IsAny<Page>(), It.IsAny<Dictionary<string, dynamic>>(), It.IsAny<FormSchema>(), It.IsAny<string>(), It.IsAny<FormAnswers>(), It.IsAny<List<object>>()), Times.Once);
//        }

//        [Fact]
//        public async Task ProcessAddAnother_ShouldReturnCorrectProcessRequestEntity_IfPageInvalid()
//        {
//            // Arrange
//            var viewModel = new Dictionary<string, dynamic>
//            {
//                {
//                    "question:0:", "answer"
//                },
//                {
//                    "question:1:", "answer"
//                }
//            };

//            var addAnotherElement = new ElementBuilder()
//                .WithType(EElementType.AddAnother)
//                .WithLabel("Person")
//                .Build();

//            var textboxElement = new ElementBuilder()
//                .WithType(EElementType.Textbox)
//                .WithLabel("Name")
//                .WithQuestionId("question")
//                .Build();

//            addAnotherElement.Properties.Elements = new List<IElement>
//            {
//                textboxElement
//            };
//            addAnotherElement.Properties.CurrentIncrementOfFieldsets = 1;

//            var page = new PageBuilder()
//                .WithElement(addAnotherElement)
//                .WithValidatedModel(true)
//                .WithPageSlug("page-one")
//                .Build();

//            var baseSchema = new FormSchemaBuilder()
//                .WithPage(page)
//                .WithBaseUrl("form")
//                .Build();

//            var dynamicFormSchema = _addAnotherSchemaTransformFactory.Transform(baseSchema);
//            var dynamicFormPage = dynamicFormSchema.Pages[0];
//            dynamicFormPage.Validate(new Dictionary<string, dynamic>(), _validators.Object, baseSchema);

//            // Act
//            var result = await _addAnotherService.ProcessAddAnother(viewModel, dynamicFormPage, baseSchema,
//                Guid.NewGuid().ToString(), "page-one");

//            // Assert
//            Assert.False(result.RedirectToAction);
//            Assert.NotNull(result.Page);
//            Assert.NotNull(result.ViewModel);
//        }
//    }
//}
