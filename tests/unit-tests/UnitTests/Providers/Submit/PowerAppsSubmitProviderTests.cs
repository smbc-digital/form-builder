using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Providers.Submit;
using form_builder.Services.MappingService.Entities;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Submit
{
    public class PowerAppsSubmitProviderTests
    {

        private readonly PowerAppsSubmitProvider _powerAppsSubmitProvider;
        private readonly Mock<IGateway> _mockGateway = new();
        private readonly MappingEntity _mappingEntity =
            new MappingEntityBuilder()
                .WithFormAnswers(new FormAnswers
                {
                    Path = "page-one",
                    Pages = new List<PageAnswers>
                    {
                        new PageAnswers
                        {
                            Answers = new List<Answers>
                            {
                                new Answers
                                {
                                    Response = "testResponse",
                                    QuestionId = "testQuestionId"
                                }
                            },
                            PageSlug = "page-one"
                        }
                    }
                })
                .WithData(new ExpandoObject())
                .Build();

        public PowerAppsSubmitProviderTests()
        {
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            _powerAppsSubmitProvider = new PowerAppsSubmitProvider(_mockGateway.Object);
        }

        [Fact]
        public async Task PostAsync_ShouldCallGateway()
        {
            SubmitSlug submitSlug = new SubmitSlug { URL = "www.test.com", Type = "flowtoken", AuthToken = "TestToken" };

            await _powerAppsSubmitProvider.PostAsync(_mappingEntity, submitSlug);

            _mockGateway.Verify(_ => _.PostAsync(It.Is<string>(x => x == submitSlug.URL), It.Is<object>(x => x == _mappingEntity.Data), It.Is<string>(x => x == submitSlug.Type), It.Is<string>(x => x == submitSlug.AuthToken)), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldReturnResponseContent()
        {
            SubmitSlug submitSlug = new SubmitSlug { URL = "www.test.com", Type = "flowtoken", AuthToken = "TestToken" };

            var result = await _powerAppsSubmitProvider.PostAsync(_mappingEntity, submitSlug);

            Assert.NotNull(result);
            Assert.IsType<HttpResponseMessage>(result);
        }
    }
}
