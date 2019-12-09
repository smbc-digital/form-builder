using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Services
{
    public class SubmitServiceTests
    {
        //public SubmitServiceTests()
        //{

        //}

        //[Fact]
        //public async Task Submit_Application_ShoudlThrowApplicationException_WhenNoSubmitUrlSpecified()
        //{
        //    // Arrange
        //    var element = new ElementBuilder()
        //         .WithType(EElementType.H1)
        //         .WithQuestionId("test-id")
        //         .WithPropertyText("test-text")
        //         .Build();

        //    var behaviour = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug(null)
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithBehaviour(behaviour)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    // Act
        //    var result = await Assert.ThrowsAsync<NullReferenceException>(() => _homeController.Submit("form"));

        //    // Assert
        //    Assert.Equal("HomeController, Submit: No postUrl supplied for submit form", result.Message);
        //    _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        //}


        //[Fact]
        //public async Task Submit_Applicaton_ShouldCatchException_WhenGatewayCallThrowsException()
        //{
        //    // Arrange
        //    var guid = Guid.NewGuid();

        //    var formData = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug("testUrl")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithBehaviour(formData)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
        //        .ThrowsAsync(new Exception("error"));

        //    // Act & Assert
        //    await Assert.ThrowsAsync<Exception>(() => _homeController.Submit("form"));
        //}

        //[Fact]
        //public async Task Submit_ShouldCallGateway_WithFormData()
        //{
        //    // Arrange
        //    var questionId = "test-question";
        //    var questionResponse = "test-response";
        //    var callbackValue = new PostData();
        //    var cacheData = new FormAnswers
        //    {
        //        Pages = new List<PageAnswers>
        //        {
        //            new PageAnswers
        //            {
        //                PageSlug = "page-one",
        //                Answers = new List<Answers>
        //                {
        //                    new Answers
        //                    {
        //                            QuestionId = questionId,
        //                            Response = questionResponse
        //                    }
        //                }
        //            }
        //        },
        //        Path = "page-one"
        //    };

        //    var formData = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug("testUrl")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithBehaviour(formData)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));
        //    _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
        //        .ReturnsAsync(new System.Net.Http.HttpResponseMessage
        //        {
        //            StatusCode = HttpStatusCode.OK
        //        })
        //        .Callback<string, object>((x, y) => callbackValue = (PostData)y);
        //    // Act
        //    await _homeController.Submit("form");

        //    // Assert
        //    _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        //    _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);

        //    Assert.NotNull(callbackValue);
        //    Assert.Equal(questionId, callbackValue.Answers[0].QuestionId);
        //    Assert.Equal(questionResponse, callbackValue.Answers[0].Response);
        //}

        //[Fact]
        //public async Task Submit_ShouldCallCacheProvider_ToGetFormData()
        //{
        //    // Arrange
        //    _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>())).ReturnsAsync(new HttpResponseMessage
        //    {
        //        StatusCode = HttpStatusCode.OK
        //    });

        //    var formData = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug("testUrl")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithBehaviour(formData)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    // Act
        //    await _homeController.Submit("form");

        //    // Assert
        //    _mockDistributedCache.Verify(_ => _.GetString(It.IsAny<string>()), Times.Once);
        //}


        //[Fact]
        //public async Task Submit_ShouldReturnErrorView_WhenGuid_IsEmpty()
        //{
        //    //Arrange 
        //    var guid = string.Empty;
        //    _mockSession.Setup(_ => _.GetSessionGuid())
        //        .Returns(guid);

        //    // Assert
        //    var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Submit("form"));
        //    Assert.Equal("A Session GUID was not provided.", result.Message);
        //}

        //[Fact]
        //public async Task Submit__Application_ShoudlThrowApplicationException_WhenGatewayResponse_IsNotOk()
        //{
        //    // Arrange
        //    var element = new ElementBuilder()
        //         .WithType(EElementType.H1)
        //         .WithQuestionId("test-id")
        //         .WithPropertyText("test-text")
        //         .Build();

        //    var behaviour = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug("test-url")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithElement(element)
        //        .WithBehaviour(behaviour)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    var cacheData = new FormAnswers
        //    {
        //        Path = "page-one"
        //    };

        //    _mockDistributedCache.Setup(_ => _.GetString(It.IsAny<string>())).Returns(JsonConvert.SerializeObject(cacheData));

        //    _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
        //        .ReturnsAsync(new HttpResponseMessage
        //        {
        //            StatusCode = HttpStatusCode.InternalServerError
        //        });

        //    // Act
        //    var result = await Assert.ThrowsAsync<ApplicationException>(() => _homeController.Submit("form"));

        //    // Assert
        //    Assert.StartsWith("HomeController, Submit: An exception has occured while attemping to call ", result.Message);
        //    _gateWay.Verify(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        //}


        //[Fact]
        //public async Task Submit_ShouldReturnView_OnSuccessfulGatewayCall_And_DeleteCacheEntry()
        //{
        //    // Arrange
        //    var guid = Guid.NewGuid();

        //    var formData = new BehaviourBuilder()
        //        .WithBehaviourType(EBehaviourType.SubmitForm)
        //        .WithPageSlug("testUrl")
        //        .Build();

        //    var page = new PageBuilder()
        //        .WithBehaviour(formData)
        //        .WithPageSlug("page-one")
        //        .Build();

        //    var schema = new FormSchemaBuilder()
        //        .WithPage(page)
        //        .Build();

        //    _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
        //        .ReturnsAsync(schema);

        //    _gateWay.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<PostData>()))
        //       .ReturnsAsync(new HttpResponseMessage
        //       {
        //           StatusCode = HttpStatusCode.OK,
        //           Content = new StringContent("\"1234456\"")
        //       });

        //    _mockSession.Setup(_ => _.GetSessionGuid())
        //        .Returns(guid.ToString());


        //    // Act
        //    var result = await _homeController.Submit("form");

        //    // Assert
        //    var viewResult = Assert.IsType<ViewResult>(result);

        //    _mockSession.Verify(_ => _.RemoveSessionGuid(), Times.Once);
        //    _mockDistributedCache.Verify(_ => _.Remove(It.Is<string>(x => x == guid.ToString())), Times.Once);
        //    Assert.Equal("Submit", viewResult.ViewName);
        //}
    }
}
