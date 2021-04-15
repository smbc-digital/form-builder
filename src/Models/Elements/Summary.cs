using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;

namespace form_builder.Models.Elements
{
    public class Summary : Element
    {
        public Summary()
        {
            Type = EElementType.Summary;
        }

        public override async Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            var htmlContent = new HtmlContentBuilder();
            var pages = elementHelper.GenerateQuestionAndAnswersList(guid, formSchema);

            if (Properties.HasSummarySectionsDefined)
            {
                var summaryViewModel = new SummarySectionsViewModel
                {
                    Sections = Properties.Sections.Select(_ => new SummarySection
                    {
                        Title = _.Title,
                        Pages = _.Pages.SelectMany(x => pages.Where(y => y.PageSlug.Equals(x))).ToList(),
                    }).ToList(),
                    AllowEditing = Properties.AllowEditing
                };

                return await viewRender.RenderAsync(Type.ToString(), summaryViewModel);
            }

            var summaryViewModelSingleSection = new SummarySectionsViewModel
            {
                AllowEditing = Properties.AllowEditing,
                Sections = new List<SummarySection>()
                    {
                        new SummarySection {
                            Pages = pages
                    }
                }
            };

            return await viewRender.RenderAsync(Type.ToString(), summaryViewModelSingleSection);
        }
    }
}