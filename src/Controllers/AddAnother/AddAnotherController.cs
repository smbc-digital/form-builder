using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Extensions;
using form_builder.Services.PageService;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers.AddAnother
{
    public class AddAnotherController : Controller
    {
        private readonly IPageService _pageService;

        public AddAnotherController(IPageService pageService) => _pageService = pageService;

        //[HttpPost]
        //[Route("{form}/{path}/add-another")]
        //public async Task<IActionResult> AddAnother(
        //    string form,
        //    string path,
        //    Dictionary<string, string[]> formData)
        //{
        //    var viewModel = formData.ToNormaliseDictionary(string.Empty);
        //    viewModel.Add("add-another", true);

        //    var currentPageResult = await _pageService.ProcessRequest(form, path, viewModel, null, ModelState.IsValid);

        //    if (!currentPageResult.Page.IsValid || currentPageResult.UseGeneratedViewModel)
        //        return View(currentPageResult.ViewName, currentPageResult.ViewModel);

        //    return RedirectToAction(currentPageResult.RedirectAction,
        //        currentPageResult.RouteValues ?? new {form, path});
        //}
    }
}
