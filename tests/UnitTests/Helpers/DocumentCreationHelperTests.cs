using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Helpers.DocumentCreation;
using form_builder.Models;
using form_builder_tests.Builders;
using Xunit;

namespace form_builder_tests.UnitTests.Helpers
{
    public class DocumentCreationHelperTests
    {
        private readonly DocumentCreationHelper _documentCreation;
        public DocumentCreationHelperTests()
        {
            _documentCreation = new DocumentCreationHelper();
        }

        [Theory]
        [InlineData(EElementType.Textbox, "question label", "test")]
        [InlineData(EElementType.Textarea, "What is your", "textAreaValue")]
        [InlineData(EElementType.Checkbox, "Checkbox label", "yes")]
        [InlineData(EElementType.DatePicker, "Enter the date", "01/01/2000")]
        [InlineData(EElementType.Radio, "Radio radio", "no")]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_AndValue_ForElements(EElementType type, string labelText, string value)
        {
            var questionId = "test-questionID";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_AndValue_ForStreetElement()
        {
            var questionId = "test-questionID";
            var labelText = "Enter the Street";
            var value = "street, city, postcode, uk";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-streetaddress-description", Response = value } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldReturnCorrectLabelText_AndValue_ForAddressElement()
        {
            var questionId = "test-questionID";
            var labelText = "Whats your Address";
            var value = "11 road, city, postcode, uk";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-address-description", Response = value } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {value}", result[0]);
        }
        
        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateList_With_FileUpload()
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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Equal(3, result.Count);
            Assert.Equal(string.Empty, result[0]);
            Assert.Equal("Files:", result[1]);
            Assert.Equal($"{labelText}: {value.TrustedOriginalFileName}", result[2]);
        }
                

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateList_With_NoFilesTitle_WhenNoFileUploadElements()
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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.DoesNotContain("Files:", result);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateList_With_AnElement_And_FileUploadElement()
        {
            var questionId = "test-questionID";
            var questionId2 = "test-questionIDtwo";
            var labelText = "Evidence file";
            var labelText2 = "FirstName";
            var value = new FileUploadModel{ TrustedOriginalFileName = "your_upload_file.txt" };
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId2, Response = "test response" } ,new Answers { QuestionId = $"{questionId}-fileupload", Response = Newtonsoft.Json.JsonConvert.SerializeObject(value) } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Equal(4, result.Count);
            Assert.Equal($"{labelText2}: test response", result[0]);
            Assert.Equal(string.Empty, result[1]);
            Assert.Equal("Files:", result[2]);
            Assert.Equal($"{labelText}: {value.TrustedOriginalFileName}", result[3]);
        }

        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateList_With_OptionalLabelText()
        {
            var questionId = "test-questionID";
            var questionId2 = "test-questionIDtwo";
            var questionId3 = "test-questionIDthree";
            var labelText = "Textarea question one";
            var labelText2 = "Textbox question two";
            var labelText3 = "Textarea question three";
            var value = "answer number one";
            var value2 = "answer number two";
            var value3= "answer number three";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = questionId, Response = value }, new Answers { QuestionId = questionId2, Response = value2  }, new Answers { QuestionId = questionId3, Response = value3  } }}}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Equal($"{labelText} (optional): {value}", result[0]);
            Assert.Equal($"{labelText2}: {value2}", result[1]);
            Assert.Equal($"{labelText3} (optional): {value3}", result[2]);
        }

           
        [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateCorrectValue_ForDateInput()
        {
            var questionId = "test-questionID";
            var labelText = "What Date do you like";
            var valueDay = "01";
            var valueMonth = "02";
            var valueYear = "2010";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-day", Response = valueDay }, new Answers { QuestionId = $"{questionId}-month", Response = valueMonth }, new Answers { QuestionId = $"{questionId}-year", Response = valueYear } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {valueDay}/{valueMonth}/{valueYear}", result[0]);
        }

                [Fact]
        public void GenerateQuestionAndAnswersDictionary_ShouldGenerateCorrectValue_ForTimeInput()
        {
            var questionId = "test-questionID";
            var labelText = "What Time do you like";
            var valueMin = "10";
            var valueHour = "31";
            var valueAmPm = "am";
            var formAnswers = new FormAnswers{ Pages = new List<PageAnswers>{ new PageAnswers { Answers = new List<Answers> { new Answers { QuestionId = $"{questionId}-hours", Response = valueMin }, new Answers { QuestionId = $"{questionId}-minutes", Response = valueHour }, new Answers { QuestionId = $"{questionId}-ampm", Response = valueAmPm } } }}};

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

            var result = _documentCreation.GenerateQuestionAndAnswersDictionary(formAnswers, formSchema);

            Assert.Single(result);
            Assert.Equal($"{labelText}: {valueMin}:{valueHour}{valueAmPm}", result[0]);
        }
    }
}
