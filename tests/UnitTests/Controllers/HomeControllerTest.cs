using System.Collections.Generic;
using form_builder.Controllers;
using form_builder.Providers;
using form_builder.Validators;
using Xunit;
using Moq;
using StockportGovUK.AspNetCore.Gateways;
using System.Threading.Tasks;
using System;
using form_builder.Models;
using Microsoft.AspNetCore.Mvc;
using form_builder.ViewModels;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;

namespace form_builder_tests.UnitTests.Controllers
{
    public class HomeControllerTest
    {
        private HomeController _homeController;
        private readonly Mock<ICacheProvider> _cacheProvider = new Mock<ICacheProvider>();
        private readonly Mock<IEnumerable<IElementValidator>> _validators = new Mock<IEnumerable<IElementValidator>>();
        private readonly Mock<ISchemaProvider> _schemaProvider = new Mock<ISchemaProvider>();
        private readonly Mock<IGateway> _gateWay = new Mock<IGateway>();
        private readonly Mock<IPageHelper> _pageHelper = new Mock<IPageHelper>();
        
        public HomeControllerTest()
        {
            _pageHelper.Setup(_ => _.GenerateHtml(It.IsAny<Page>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<FormSchema>()))
             .ReturnsAsync(new FormBuilderViewModel());


            _homeController = new HomeController(_cacheProvider.Object, _validators.Object, _schemaProvider.Object, _gateWay.Object, _pageHelper.Object);
        }

        [Fact]
        public async Task Index_ShouldCallSchemaProvider_ToGetFormSchema()
        {
            var result = await _homeController.Index("form", "page-one", Guid.NewGuid());

            _schemaProvider.Verify(_ => _.Get<FormSchema>(It.Is<string>(x => x == "form")));
        }

        [Fact]
        public async Task Index_ShouldGenerateGuidWhenGuidIsEmpty()
        {
            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema
                {
                    Pages = new List<Page>
                    {
                        new Page
                        {
                            PageURL = "page-one",
                            Elements = new List<Element>
                            {
                                 new Element
                                 {
                                     Type = EElementType.H1,
                                     Properties = new Property
                                     {
                                         QuestionId = "test-id",
                                          Text = "test-text"
                                     }
                                 }
                            }
                        }
                    }
                });

            var result = await _homeController.Index("form", "page-one", Guid.Empty);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = (FormBuilderViewModel)viewResult.Model;

            Assert.NotEqual(Guid.Empty, model.Guid);
        }

        [Fact]
        public async Task Index_ShouldRedirectToError_WhenPageIsNotWithin_FormSchema()
        {
            _schemaProvider.Setup(_ => _.Get<FormSchema>(It.IsAny<string>()))
                .ReturnsAsync(new FormSchema
                {
                    Pages = new List<Page>
                    {
                        new Page
                        {
                            PageURL = "page-one",
                            Elements = new List<Element>
                            {
                                 new Element
                                 {
                                     Type = EElementType.H1,
                                     Properties = new Property
                                     {
                                         QuestionId = "test-id",
                                          Text = "test-text"
                                     }
                                 }
                            }
                        }
                    }
                });

            var result = await _homeController.Index("form", "non-existance-page", Guid.Empty);

            var viewResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Error", viewResult.ActionName);
        }
    }
}
