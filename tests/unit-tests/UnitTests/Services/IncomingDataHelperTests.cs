using System;
using System.Collections.Generic;
using form_builder.Enum;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Models;
using form_builder_tests.Builders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class IncomingDataHelperTests
    {
        private readonly IncomingDataHelper _helper = new();

        [Fact]
        public void AddIncomingFormDataValues_Post_ShouldThrowException_WhenIncomingValueIsNull()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("testName")
                .WithHttpActionType(EHttpActionType.Post)
                .WithOptional(false)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>();

            // Act & Assert
            var result = Assert.Throws<Exception>(() => _helper.AddIncomingFormDataValues(page, formData));
            Assert.Equal("IncomingDataHelper::AddIncomingFormDataValues, FormData does not contain testName required value",
                result.Message);
        }

        [Fact]
        public void AddIncomingFormDataValues_Post_ShouldReturnSingleObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithHttpActionType(EHttpActionType.Post)
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"}
            };

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Post_ShouldReturnSingleObject_WithOptionalTrue()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var formData = new Dictionary<string, dynamic>();

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void AddIncomingFormDataValues_Post_ShouldReturnMultipleValuesObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2")
                .WithName("nameTest2")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"},
                {"nameTest2", "-2.345"}
            };

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
            Assert.True(result.ContainsKey("questionIdTest2"));
            Assert.True(result.ContainsValue("-2.345"));
            Assert.False(result.ContainsKey("nameTest2"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Post_ShouldCall_RecursiveCheckAndCreate_AndReturnCorrectObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Post)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2.nameTest2")
                .WithName("nameTest2")
                .WithHttpActionType(EHttpActionType.Post)

                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();

            var formData = new Dictionary<string, dynamic>
            {
                {"nameTest", "45.23645"},
                {"questionIdTest2.nameTest2", "-2.345"}
            };

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
            Assert.True(result.ContainsKey("questionIdTest2.nameTest2"));
            Assert.True(result.ContainsValue("-2.345"));
            Assert.False(result.ContainsKey("nameTest2"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldThrowException_WhenIncomingValueIsNull()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("testQuestionId")
                .WithName("testName")
                .WithHttpActionType(EHttpActionType.Get)
                .WithOptional(false)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var queryData = new QueryCollection();

            // Act & Assert
            var result = Assert.Throws<Exception>(() => _helper.AddIncomingFormDataValues(page, queryData, new FormAnswers()));
            Assert.Equal("IncomingDataHelper::AddIncomingFormDataValues, FormData does not contain testName required value", result.Message);
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldReturnSingleObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithHttpActionType(EHttpActionType.Get)
                .WithOptional(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var queryData = new Dictionary<string, StringValues>
            {
                {"nameTest", new StringValues("45.23645")}
            };

            var formData = new QueryCollection(queryData);

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData, new FormAnswers());

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.False(result.ContainsKey("nameTest"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldReturn_EmptyObject_WhenOptional_AndValueNotSupplied()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            // Act
            var result = _helper.AddIncomingFormDataValues(page, new QueryCollection(), new FormAnswers());

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldReturnMultipleValuesObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2")
                .WithName("nameTest2")
                .WithOptional(true)
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();


            var queryData = new Dictionary<string, StringValues>
            {
                {"nameTest", new StringValues("45.23645")},
                {"nameTest2", new StringValues("-2.345")},
            };

            var formData = new QueryCollection(queryData);

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData, new FormAnswers());

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.True(result.ContainsKey("questionIdTest2"));
            Assert.True(result.ContainsValue("-2.345"));
            Assert.False(result.ContainsKey("nameTest"));
            Assert.False(result.ContainsKey("nameTest2"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldCall_RecursiveCheckAndCreate_AndReturnCorrectObject()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest")
                .WithName("nameTest")
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var incomingValue2 = new IncomingValuesBuilder()
                .WithQuestionId("questionIdTest2.nameTest2")
                .WithName("nameTest2")
                .WithHttpActionType(EHttpActionType.Get)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .WithIncomingValue(incomingValue2)
                .Build();

            var queryData = new Dictionary<string, StringValues>
            {
                {"nameTest", new StringValues("45.23645")},
                {"nameTest2", new StringValues("-2.345")},
            };

            var formData = new QueryCollection(queryData);

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData, new FormAnswers());

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result.ContainsKey("questionIdTest"));
            Assert.True(result.ContainsValue("45.23645"));
            Assert.True(result.ContainsKey("questionIdTest2"));
            var innerValue = result["questionIdTest2"];
            Assert.Equal("-2.345", innerValue.nameTest2);
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldDecodeData_WhenBase64Encoded_IsTrue()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("test")
                .WithName("baseEncodeTest")
                .WithHttpActionType(EHttpActionType.Get)
                .WithBase64Encoding(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var queryData = new Dictionary<string, StringValues>
            {
                {"baseEncodeTest", new StringValues("MTIzNDU2VGVzdA==")}
            };

            var formData = new QueryCollection(queryData);

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData, new FormAnswers());

            // Assert
            Assert.Single(result);
            Assert.True(result.ContainsKey("test"));
            Assert.True(result.ContainsValue("123456Test"));
        }

        [Fact]
        public void AddIncomingFormDataValues_Get_ShouldCheckIfData_IsAlreadyStored_IfNotIn_QueryParameters()
        {
            // Arrange
            var behaviour = new BehaviourBuilder()
                .WithBehaviourType(EBehaviourType.GoToPage)
                .WithPageSlug("test-test")
                .Build();

            var incomingValue = new IncomingValuesBuilder()
                .WithQuestionId("test")
                .WithName("baseEncodeTest")
                .WithHttpActionType(EHttpActionType.Get)
                .WithBase64Encoding(true)
                .Build();

            var page = new PageBuilder()
                .WithBehaviour(behaviour)
                .WithIncomingValue(incomingValue)
                .Build();

            var queryData = new Dictionary<string, StringValues>();

            var formData = new QueryCollection(queryData);

            var formAnswers = new FormAnswers
            {
                AdditionalFormData = new Dictionary<string, object>
                {
                    { "test", "11111" }
                }
            };

            // Act
            var result = _helper.AddIncomingFormDataValues(page, formData, formAnswers);

            // Assert
            Assert.Empty(result);
        }
    }
}
