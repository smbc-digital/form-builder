using System.Collections.Generic;
using System.Linq;
using form_builder.Builders.Document;
using form_builder.Extensions;
using form_builder.Mappers;
using form_builder.Models;

namespace form_builder.Helpers.DocumentCreation
{
    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        private readonly IElementMapper _elementMapper;
        public DocumentCreationHelper(IElementMapper elementMapper) => _elementMapper = elementMapper;

        public List<string> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema)
        {
            var summaryBuilder = new SummaryAnswerBuilder();
            var reducedAnswers = FormAnswersExtensions.GetReducedAnswers(formAnswers, formSchema);

            foreach (var page in formSchema.Pages.ToList())
            {
                var formSchemaQuestions = page.ValidatableElements
                    .Where(_ => _ != null)
                    .ToList();

                if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug == page.PageSlug).Select(p => p).Any())
                    continue;

                formSchemaQuestions.ForEach(question =>
                {
                    var answer = _elementMapper.GetAnswerStringValue(question, formAnswers);
                    summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);

                    summaryBuilder.AddBlankLine();
                });
            }

            return summaryBuilder.Build();
        }
    }
}