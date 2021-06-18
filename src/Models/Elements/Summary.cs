﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;
using form_builder.Models.Properties.ElementProperties;
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

            if (formSchema.Pages.Any(_ => _.Elements.Any(_ => _.Type == EElementType.AddAnother)))
            {
                var slugs = new List<List<string>>();
                var currentSlugList = new List<string>();
                foreach (var schemaPage in formSchema.Pages)
                {
                    if (schemaPage.Elements.Any(_ => _.Type == EElementType.AddAnother))
                    {
                        currentSlugList.Add(schemaPage.PageSlug);
                        slugs.Add(currentSlugList);
                        currentSlugList = new List<string>();
                        slugs.Add(new List<string> { $"{schemaPage.Elements.FirstOrDefault(_ => _.Type == EElementType.AddAnother).Properties.QuestionId}" });
                    }
                    else
                    {
                        currentSlugList.Add(schemaPage.PageSlug);
                    }
                }

                slugs.Add(currentSlugList);

                if (!Properties.HasSummarySectionsDefined)
                {
                    Properties.Sections = new List<Section>();
                    foreach (var listOfPageSlugs in slugs)
                    {
                        Properties.Sections.Add(new Section
                        {
                            Pages = listOfPageSlugs
                        });
                    }
                }

                var addAnotherPages = formSchema.Pages.Where(_ => _.Elements.Any(_ => _.Type == EElementType.AddAnother)).ToList();
                var addAnotherElements = addAnotherPages.SelectMany(_ => _.Elements.Where(_ => _.Type == EElementType.AddAnother)).ToList();

                foreach (var element in addAnotherElements)
                {
                    var formDataIncrementKey = $"addAnotherFieldset-{element.Properties.QuestionId}";
                    var currentIncrement = formAnswers.FormData.ContainsKey(formDataIncrementKey) ? int.Parse(formAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString()) : 1;
                    var addAnotherSection = Properties.Sections.FirstOrDefault(_ => _.Pages.Any(_ => _.Contains(element.Properties.QuestionId)));
                    var indexOf = Properties.Sections.IndexOf(addAnotherSection);

                    for (var i = 1; i <= currentIncrement; i++)
                    {
                        Properties.Sections.Insert(indexOf + i, new Section
                        {
                            Pages = new List<string>
                            {
                                $"{element.Properties.QuestionId}-{i}"
                            },
                            Title = element.Properties.Label
                        });
                    }

                    Properties.Sections.Remove(addAnotherSection);
                }

            }
            
            if (Properties.HasSummarySectionsDefined)
            {
                var summaryViewModel = new SummarySectionsViewModel
                {
                    Sections = Properties.Sections.Select(_ => new SummarySection
                    {
                        Title = _.Title,
                        Pages = _.Pages.SelectMany(x => pages.Where(y => y.PageSlug.Equals(x) && y.Answers.Count > 0)).ToList()
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