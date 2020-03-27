using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;

namespace form_builder.ContentFactory
{
    public class PageContentFactory
    {
        private readonly IPageHelper _pageHelper;

        public PageContentFactory(IPageHelper pageHelper)
        {
            _pageHelper = pageHelper;
        }

        public async Task<string> Build(Page page, FormSchema baseForm, string sessionGuid)
        {
            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, dynamic>(), baseForm, sessionGuid);           

            return viewModel.RawHTML;
        }
    }
}