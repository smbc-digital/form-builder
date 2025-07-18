using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder_tests.Builders;
using Moq;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
	public class DocumentCreationHelperTests
	{
		private readonly DocumentCreationHelper _documentCreation;
		private readonly Mock<IElementMapper> _mockElementMapper = new();

		public DocumentCreationHelperTests()
		{
			_documentCreation = new DocumentCreationHelper(_mockElementMapper.Object);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_Summary_With_SingleAnswer_And_NewLine()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };

			var element = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionId")
				.WithLabel("I am a label")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithPageSlug("page-one")
				.WithElement(element)
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal(2, result.Count);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldRemove_OtherPages_NotRelated_ToJourney_WhenBuilding_Answers()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-two", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-three", Answers = new List<Answers>() } } };

			var element = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdOne")
				.WithLabel("I am a label")
				.Build();

			var element2 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdtwo")
				.WithLabel("I am a label")
				.Build();

			var element3 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdthree")
				.WithLabel("I am a label")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-three")
				.Build();

			var condition = new ConditionBuilder()
				.WithConditionType(ECondition.EqualTo)
				.WithQuestionId("QuestionIdOne")
				.WithComparisonValue("orange")
				.Build();

			var behaviour2 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithCondition(condition)
				.WithPageSlug("page-two")
				.Build();

			var behaviour3 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-three")
				.Build();

			var behaviour4 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithPageSlug("page-one")
				.WithElement(element)
				.WithBehaviour(behaviour)
				.WithBehaviour(behaviour2)
				.Build();

			var page2 = new PageBuilder()
				.WithPageSlug("page-two")
				.WithElement(element2)
				.WithBehaviour(behaviour3)
				.Build();

			var page3 = new PageBuilder()
				.WithPageSlug("page-three")
				.WithElement(element3)
				.WithBehaviour(behaviour4)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.WithPage(page2)
				.WithPage(page3)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal(4, result.Count);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_FilesData()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			var value = "file.txt";
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var labelText = "I am a label";
			var labelText2 = "second label";

			var element = new ElementBuilder()
				.WithType(EElementType.MultipleFileUpload)
				.WithQuestionId("testQuestion1")
				.WithLabel("I am a label")
				.Build();

			var element2 = new ElementBuilder()
				.WithType(EElementType.FileUpload)
				.WithQuestionId("testQuestion2")
				.WithLabel(labelText2)
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithElement(element2)
				.WithBehaviour(behaviour)
				.WithPageSlug("page-one")
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal($"<b>{labelText}</b><br/>{value}", result[4]);
			Assert.Equal($"<b>{labelText2}</b><br/>{value}", result[5]);
		}

		[Theory]
		[InlineData(EElementType.Textbox)]
		[InlineData(EElementType.Textarea)]
		[InlineData(EElementType.Checkbox)]
		[InlineData(EElementType.Radio)]
		[InlineData(EElementType.DateInput)]
		[InlineData(EElementType.DatePicker)]
		[InlineData(EElementType.Select)]
		[InlineData(EElementType.TimeInput)]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_ListWithSingleItem_For_ElementType(EElementType type)
		{
			// Arrange
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			var value = "value";
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var labelText = "I am a label";

			var element = new ElementBuilder()
				.WithType(type)
				.WithQuestionId("testQuestion")
				.WithLabel(labelText)
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithBehaviour(behaviour)
				.WithPageSlug("page-one")
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal(2, result.Count);
			Assert.Equal($"<b>{labelText}</b><br/>{value}", result[0]);
		}

		[Theory]
		[InlineData(EElementType.Address)]
		[InlineData(EElementType.Street)]
		[InlineData(EElementType.Organisation)]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_TitleAsLabel_For_ElementType(EElementType type)
		{
			// Arrange
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			var value = "value";
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var titleText = "I am a title";

			var element = new ElementBuilder()
				.WithType(type)
				.WithQuestionId("testQuestion")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithPageTitle(titleText)
				.WithPageSlug("page-one")
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal(2, result.Count);
			Assert.Equal($"<b>{titleText}</b><br/>{value}", result[0]);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_ListOfMultipleItems()
		{
			// Arrange
			var value = "value";
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var labelText = "I am a label";
			var labelText2 = "second label";
			var labelText3 = "third label";

			var element = new ElementBuilder()
				.WithType(EElementType.Textbox)
				.WithQuestionId("testQuestion1")
				.WithLabel(labelText)
				.Build();

			var element2 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("testQuestion2")
				.WithLabel(labelText2)
				.Build();

			var element3 = new ElementBuilder()
				.WithType(EElementType.Select)
				.WithQuestionId("testQuestion3")
				.WithLabel(labelText3)
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithElement(element2)
				.WithElement(element3)
				.WithPageSlug("page-one")
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

			Assert.Equal(6, result.Count);
			Assert.Equal($"<b>{labelText}</b><br/>{value}", result[0]);
			Assert.Equal($"<b>{labelText2}</b><br/>{value}", result[2]);
			Assert.Equal($"<b>{labelText3}</b><br/>{value}", result[4]);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersListForPdf_ShouldReturn_Summary_With_Label_SingleAnswer_And_NewLine()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };

			var element = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionId")
				.WithLabel("I am a label")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithPageSlug("page-one")
				.WithElement(element)
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

			Assert.Equal(3, result.Count);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersListForPdf_ShouldRemove_OtherPages_NotRelated_ToJourney_WhenBuilding_Answers()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-two", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-three", Answers = new List<Answers>() } } };

			var element = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdOne")
				.WithLabel("I am a label")
				.Build();

			var element2 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdtwo")
				.WithLabel("I am a label")
				.Build();

			var element3 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdthree")
				.WithLabel("I am a label")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-three")
				.Build();

			var condition = new ConditionBuilder()
				.WithConditionType(ECondition.EqualTo)
				.WithQuestionId("QuestionIdOne")
				.WithComparisonValue("orange")
				.Build();

			var behaviour2 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithCondition(condition)
				.WithPageSlug("page-two")
				.Build();

			var behaviour3 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-three")
				.Build();

			var behaviour4 = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithPageSlug("page-one")
				.WithElement(element)
				.WithBehaviour(behaviour)
				.WithBehaviour(behaviour2)
				.Build();

			var page2 = new PageBuilder()
				.WithPageSlug("page-two")
				.WithElement(element2)
				.WithBehaviour(behaviour3)
				.Build();

			var page3 = new PageBuilder()
				.WithPageSlug("page-three")
				.WithElement(element3)
				.WithBehaviour(behaviour4)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.WithPage(page2)
				.WithPage(page3)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

			Assert.Equal(6, result.Count);
		}

		[Theory]
		[InlineData(EElementType.Textbox)]
		[InlineData(EElementType.Textarea)]
		[InlineData(EElementType.Checkbox)]
		[InlineData(EElementType.Radio)]
		[InlineData(EElementType.DateInput)]
		[InlineData(EElementType.DatePicker)]
		[InlineData(EElementType.Select)]
		[InlineData(EElementType.TimeInput)]
		public async Task GenerateQuestionAndAnswersListForPdf_ShouldReturn_ListWithLabel_And_SingleItem_For_ElementType(EElementType type)
		{
			// Arrange
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			var value = "value";
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var labelText = "I am a label";

			var element = new ElementBuilder()
				.WithType(type)
				.WithQuestionId("testQuestion")
				.WithLabel(labelText)
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithBehaviour(behaviour)
				.WithPageSlug("page-one")
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

			Assert.Equal(3, result.Count);
			Assert.Equal($"{labelText}", result[0]);
			Assert.Equal($"{value}", result[1]);
		}

		[Theory]
		[InlineData(EElementType.Address)]
		[InlineData(EElementType.Street)]
		[InlineData(EElementType.Organisation)]
		public async Task GenerateQuestionAndAnswersListForPdf_ShouldReturn_TitleAsLabel_For_ElementType(EElementType type)
		{
			// Arrange
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			var value = "value";
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var titleText = "I am a title";

			var element = new ElementBuilder()
				.WithType(type)
				.WithQuestionId("testQuestion")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithPageTitle(titleText)
				.WithPageSlug("page-one")
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

			Assert.Equal(3, result.Count);
			Assert.Equal($"{titleText}", result[0]);
			Assert.Equal($"{value}", result[1]);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersListForPdf_ShouldReturn_ListOfMultipleItems()
		{
			// Arrange
			var value = "value";
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };
			_mockElementMapper
				.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>()))
				.ReturnsAsync(value);

			var labelText = "I am a label";
			var labelText2 = "second label";
			var labelText3 = "third label";

			var element = new ElementBuilder()
				.WithType(EElementType.Textbox)
				.WithQuestionId("testQuestion1")
				.WithLabel(labelText)
				.Build();

			var element2 = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("testQuestion2")
				.WithLabel(labelText2)
				.Build();

			var element3 = new ElementBuilder()
				.WithType(EElementType.Select)
				.WithQuestionId("testQuestion3")
				.WithLabel(labelText3)
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithElement(element)
				.WithElement(element2)
				.WithElement(element3)
				.WithPageSlug("page-one")
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersListForPdf(formAnswers, formSchema);

			Assert.Equal(9, result.Count);
			Assert.Equal($"{labelText}", result[0]);
			Assert.Equal($"{value}", result[1]);
			Assert.Equal($"{labelText2}", result[3]);
			Assert.Equal($"{value}", result[4]);
			Assert.Equal($"{labelText3}", result[6]);
			Assert.Equal($"{value}", result[7]);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_Summary_With_SingleAnswer_And_NewLine_And_PageTitle()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() } } };

			var element = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionId")
				.WithLabel("I am a label")
				.Build();

			var behaviour = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			var page = new PageBuilder()
				.WithPageTitle("Page one")
				.WithPageSlug("page-one")
				.WithElement(element)
				.WithBehaviour(behaviour)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(page)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema, true);

			Assert.Equal(3, result.Count);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_Summary_With_SingleAnswer_And_NewLine_And_PageTitle_Testing_TwoPages()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-two", Answers = new List<Answers>() } } };

			Element elementOne = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdOne")
				.WithLabel("I am a label")
				.Build();

			Element elementTwo = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdTwo")
				.WithLabel("I am also a label")
				.Build();

			Behaviour behaviourNextPage = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-two")
				.Build();

			Behaviour behaviourSubmit = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			Page pageOne = new PageBuilder()
				.WithPageTitle("Page one")
				.WithPageSlug("page-one")
				.WithElement(elementOne)
				.WithBehaviour(behaviourNextPage)
				.Build();

			Page pageTwo = new PageBuilder()
				.WithPageTitle("Page two")
				.WithPageSlug("page-two")
				.WithElement(elementTwo)
				.WithBehaviour(behaviourSubmit)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(pageOne)
				.WithPage(pageTwo)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema, true);

			Assert.Equal(6, result.Count);
		}

		[Fact]
		public async Task GenerateQuestionAndAnswersList_ShouldReturn_Summary_With_SingleAnswer_And_NewLine_And_PageTitle_Testing_ThreePages_OneBeingEmptyOfAnswers()
		{
			// Arrange
			_mockElementMapper.Setup(_ => _.GetAnswerStringValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).ReturnsAsync("test value");
			var formAnswers = new FormAnswers { Pages = new List<PageAnswers> { new PageAnswers { PageSlug = "page-one", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-two", Answers = new List<Answers>() }, new PageAnswers { PageSlug = "page-three", Answers = new List<Answers>() } } };

			Element elementOne = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdOne")
				.WithLabel("I am a label")
				.Build();

			Element elementTwo = new ElementBuilder()
				.WithType(EElementType.Textarea)
				.WithQuestionId("QuestionIdTwo")
				.WithLabel("I am also a label")
				.Build();

			Element elementThree = new ElementBuilder()
				.WithType(EElementType.H2)
				.WithLabel("I am also a label")
				.Build();

			Behaviour behaviourNextPage = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-two")
				.Build();

			Behaviour behaviourNextPageTwo = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.GoToPage)
				.WithPageSlug("page-three")
				.Build();

			Behaviour behaviourSubmit = new BehaviourBuilder()
				.WithBehaviourType(EBehaviourType.SubmitForm)
				.Build();

			Page pageOne = new PageBuilder()
				.WithPageTitle("Page one")
				.WithPageSlug("page-one")
				.WithElement(elementOne)
				.WithBehaviour(behaviourNextPage)
				.Build();

			Page pageTwo = new PageBuilder()
				.WithPageTitle("Page two")
				.WithPageSlug("page-two")
				.WithElement(elementTwo)
				.WithBehaviour(behaviourNextPageTwo)
				.Build();

			Page pageThree = new PageBuilder()
				.WithPageTitle("Page three")
				.WithPageSlug("page-three")
				.WithElement(elementThree)
				.WithBehaviour(behaviourSubmit)
				.Build();

			var formSchema = new FormSchemaBuilder()
				.WithPage(pageOne)
				.WithPage(pageTwo)
				.WithPage(pageThree)
				.Build();

			// Act
			var result = await _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema, true);

			Assert.Equal(6, result.Count);
		}
	}
}