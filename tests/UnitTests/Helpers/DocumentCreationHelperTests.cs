using System;
using System.Collections.Generic;
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
        private readonly Mock<IElementMapper> _mockElementMapper = new Mock<IElementMapper>();

        public DocumentCreationHelperTests()
        {
            _documentCreation = new DocumentCreationHelper(_mockElementMapper.Object);
        }

        [Theory(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        [InlineData(EElementType.Textbox, "question label", "test")]
        [InlineData(EElementType.Textarea, "What is your", "textAreaValue")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForElements(EElementType type, string labelText, string value)
        {
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(value);

            var element = new ElementBuilder()
                            .WithType(type)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForDatePickerElement()
        {
            var questionId = "test-questionID";
            var labelText = "Enter the date";
            var value = new DateTime(2000, 01, 01);
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(value);

            var element = new ElementBuilder()
                            .WithType(EElementType.DatePicker)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value.ToString("dd/MM/yyyy")}", result[0]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForCheckboxElement()
        {
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(new List<string>{"yes"});
            
            var questionId = "test-questionID";
            var labelText = "Checkbox label";
            var labelValue =  "Yes Text";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};

            var element = new ElementBuilder()
                            .WithType(EElementType.Checkbox)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .WithOptions(new List<Option>{ new Option{ Text = labelValue, Value = "yes" }, new Option{ Text = "No Text", Value = "n" }})
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {labelValue}", result[0]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForRadioElement()
        {
             var labelText = "Radio radio";
             var labelValue = "No Text";
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns("n");
            
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};

            var element = new ElementBuilder()
                            .WithType(EElementType.Radio)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .WithOptions(new List<Option>{ new Option{ Text = "Yes Text", Value = "yes" }, new Option{ Text = labelValue, Value = "n" }})
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {labelValue}", result[0]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForStreetElement()
        {
             var value = new StockportGovUK.NetStandard.Models.Addresses.Address{ SelectedAddress = "street, city, postcode, uk" };
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(value);

            var questionId = "test-questionID";
            var labelText = "Enter the Street";
            var formAnswers =  new FormAnswers{ Pages = new List<PageAnswers>()};

            var element = new ElementBuilder()
                            .WithType(EElementType.Street)
                            .WithQuestionId(questionId)
                            .WithStreetLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value.SelectedAddress}", result[0]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldReturnCorrectLabelText_AndValue_ForAddressElement()
        {
            var value = new StockportGovUK.NetStandard.Models.Addresses.Address{ SelectedAddress = "11 road, city, postcode, uk" };
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(value);

            var questionId = "test-questionID";
            var labelText = "Whats your Address";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};

            var element = new ElementBuilder()
                            .WithType(EElementType.Address)
                            .WithQuestionId(questionId)
                            .WithAddressLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value.SelectedAddress}", result[0]);
        }
        
        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateList_With_FileUpload()
        {
            var questionId = "test-questionID";
            var labelText = "Evidence file";
            var value = new FileUploadModel{ TrustedOriginalFileName = "your_upload_file.txt" };
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } }}};

            var element = new ElementBuilder()
                            .WithType(EElementType.FileUpload)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var page = new PageBuilder()
                        .WithElement(element)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Equal(3, result.Count);
            Assert.Equal(string.Empty, result[0]);
            Assert.Equal("Files:", result[1]);
            Assert.Equal($"{labelText}: {value.TrustedOriginalFileName}", result[2]);
        }
                

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateList_With_NoFilesTitle_WhenNoFileUploadElements()
        {
            var value = "answer number one";
            var value2 = "answer number two";
            var value3= "answer number three";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = "test-questionID", Response = value }, new Answers { QuestionId = "test-questionIDtwo", Response = value2  }, new Answers { QuestionId = "test-questionIDthree", Response = value3  } }}}};

            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("test-questionID")
                .WithLabel("Textarea question one")
                .Build();


            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId("test-questionIDtwo")
                .WithLabel("Textbox question two")
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId("test-questionIDthree")
                .WithLabel("Textarea question three")
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.DoesNotContain("Files:", result);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateList_With_AnElement_And_FileUploadElement()
        {
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns("test response");

            var questionId = "test-questionID";
            var questionId2 = "test-questionIDtwo";
            var labelText = "Evidence file";
            var labelText2 = "FirstName";
            var value = new FileUploadModel{ TrustedOriginalFileName = "your_upload_file.txt" };
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } }}};

            var element = new ElementBuilder()
                            .WithType(EElementType.FileUpload)
                            .WithQuestionId(questionId)
                            .WithLabel(labelText)
                            .Build();

            var element2 = new ElementBuilder()
                            .WithType(EElementType.Textarea)
                            .WithQuestionId(questionId2)
                            .WithLabel(labelText2)
                            .Build();


            var page = new PageBuilder()
                        .WithElement(element)
                        .WithElement(element2)
                        .Build();

            var formSchema = new FormSchemaBuilder()
                            .WithPage(page)
                            .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

             Assert.Equal(4, result.Count);
            Assert.Equal($"{labelText2}: test response", result[0]);
            Assert.Equal(string.Empty, result[1]);
            Assert.Equal("Files:", result[2]);
            Assert.Equal($"{labelText}: {value.TrustedOriginalFileName}", result[3]);
        }

        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateList_With_OptionalLabelText()
        {


            var questionId = "test-questionID";
            var questionId2 = "test-questionIDtwo";
            var questionId3 = "test-questionIDthree";
            var labelText = "Textarea question one";
            var labelText2 = "Textbox question two";
            var labelText3 = "Textarea question three";
            var value = "answer number one";
            var value2 = "answer number two";
            var value3 = "answer number three";
            
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Type == EElementType.Textarea && x.Properties.QuestionId == questionId), It.IsAny<FormAnswers>())).Returns(value);
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Type == EElementType.Textbox), It.IsAny<FormAnswers>())).Returns(value2);
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.Is<IElement>(x => x.Type == EElementType.Textarea && x.Properties.QuestionId == questionId3), It.IsAny<FormAnswers>())).Returns(value3);
            
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};

            var element = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId(questionId)
                .WithOptional(true)
                .WithLabel(labelText)
                .Build();


            var element2 = new ElementBuilder()
                .WithType(EElementType.Textbox)
                .WithQuestionId(questionId2)
                .WithLabel(labelText2)
                .Build();

            var element3 = new ElementBuilder()
                .WithType(EElementType.Textarea)
                .WithQuestionId(questionId3)
                .WithOptional(true)
                .WithLabel(labelText3)
                .Build();

            var page = new PageBuilder()
                .WithElement(element)
                .WithElement(element2)
                .WithElement(element3)
                .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Equal($"{labelText} (optional): {value}", result[0]);
            Assert.Equal($"{labelText2}: {value2}", result[1]);
            Assert.Equal($"{labelText3} (optional): {value3}", result[2]);
        }

           
        [Fact(Skip="Logic moved to ElementMapper, tests need to be refactored.")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateCorrectValue_ForDateInput()
        {
            var questionId = "test-questionID";
            var labelText = "What Date do you like";
            var valueDay = "01";
            var valueMonth = "02";
            var valueYear = "2010";
            var value = new DateTime(2010,02,01);
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>()};

            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(value);
            
            var element = new ElementBuilder()
                .WithType(EElementType.DateInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            var page = new PageBuilder()
            .WithElement(element)
            .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {valueDay}/{valueMonth}/{valueYear}", result[0]);
        }

        [Theory(Skip = "Logic moved to ElementMapper, tests need to be refactored.")]
        [InlineData("03", "10", "PM")]
        [InlineData("12", "54", "PM")]
        [InlineData("12", "11", "AM")]
        [InlineData("05", "23", "AM")]
        public void GenerateQuestionAndAnswersList_ShouldGenerateCorrectValue_ForTimeInput(string hour, string min, string amPm)
        {
            var dateTime = DateTime.Parse($"{hour}:{min} {amPm}");
            _mockElementMapper.Setup(_ => _.GetAnswerValue(It.IsAny<IElement>(), It.IsAny<FormAnswers>())).Returns(dateTime.TimeOfDay);

            var questionId = "test-questionID";
            var labelText = "What Time do you like";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-hours", Response = min }, new Answers { QuestionId = $"{questionId}-minutes", Response = hour }, new Answers { QuestionId = $"{questionId}-ampm", Response = amPm } } }}};

            var element = new ElementBuilder()
                .WithType(EElementType.TimeInput)
                .WithQuestionId(questionId)
                .WithLabel(labelText)
                .Build();

            var page = new PageBuilder()
            .WithElement(element)
            .Build();

            var formSchema = new FormSchemaBuilder()
                .WithPage(page)
                .Build();

            var result = _documentCreation.GenerateQuestionAndAnswersList(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {hour}:{min} {amPm}", result[0]);
        }
    }
}
