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

        public async Task<List<string>> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema)
        {
            var summaryBuilder = new SummaryAnswerBuilder();
            var reducedAnswers = FormAnswersExtensions.GetReducedAnswers(formAnswers, formSchema);

            if (!string.IsNullOrEmpty(formAnswers.CaseReference))
            {
                summaryBuilder.Add("Case Reference", formAnswers.CaseReference, Enum.EElementType.Textbox);
                summaryBuilder.AddBlankLine();
            }

			foreach (var page in formSchema.Pages.ToList())
            {
                var formSchemaQuestions = page.ValidatableElements
                    .Where(_ => _ is not null)
                    .ToList();

                if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug.Equals(page.PageSlug)).Select(p => p).Any())
                    continue;

                formSchemaQuestions.ForEach(async question =>
                {
                    var answer = await _elementMapper.GetAnswerStringValue(question, formAnswers);
                    summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);

                    summaryBuilder.AddBlankLine();
                });
            }

            return summaryBuilder.Build();
        }
    }
}