using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Submit;
using form_builder.Services.MappingService.Entities;
using form_builder_tests.Builders;
using Moq;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.Response;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace form_builder_tests.UnitTests.Providers.Submit {
    public class AuthenticationHeaderSubmitProviderTests {

        private readonly AuthenticationHeaderSubmitProvider _authenticationHeaderSubmitProvider;
        private readonly Mock<IGateway> _mockGateway = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _mockPageHelper = new Mock<IPageHelper>();
        private readonly MappingEntity _mappingEntity =
            new MappingEntityBuilder()
                .WithFormAnswers(new FormAnswers {
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

        public AuthenticationHeaderSubmitProviderTests() {
            _mockGateway.Setup(_ => _.PostAsync(It.IsAny<string>(), It.IsAny<object>()))
                .ReturnsAsync(new HttpResponseMessage { StatusCode = System.Net.HttpStatusCode.OK });

            _authenticationHeaderSubmitProvider = new AuthenticationHeaderSubmitProvider(_mockGateway.Object);
        }

        [Fact]
        public async Task PostAsync_ShouldCallGateway() {
            SubmitSlug submitSlug = new SubmitSlug { URL = "www.test.com" };

            await _authenticationHeaderSubmitProvider.PostAsync(_mappingEntity, submitSlug);

            _mockGateway.Verify(_ => _.PostAsync(It.Is<string>(x => x == submitSlug.URL), It.Is<object>(x => x == _mappingEntity.Data)), Times.Once);
        }

        [Fact]
        public async Task PostAsync_ShouldReturnResponseContent() {
            SubmitSlug submitSlug = new SubmitSlug { URL = "www.test.com" };

            var result = await _authenticationHeaderSubmitProvider.PostAsync(_mappingEntity, submitSlug);

            Assert.NotNull(result);
            Assert.IsType<HttpResponseMessage>(result);
        }
    }
}
