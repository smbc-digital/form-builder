using form_builder.Builders.Document;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Mappers;
using form_builder.Models;

namespace form_builder.Helpers.DocumentCreation
{
    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        private readonly IElementMapper _elementMapper;
        public DocumentCreationHelper(IElementMapper elementMapper) => _elementMapper = elementMapper;

        public async Task<List<string>> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema, bool withPageTitles = false)
        {
            var summaryBuilder = new SummaryAnswerBuilder();
            var reducedAnswers = formAnswers.GetReducedAnswers(formSchema);

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

                string answers = string.Empty;
				
				formSchemaQuestions.ForEach(async question =>
				{
					answers += await _elementMapper.GetAnswerStringValue(question, formAnswers);
				});

                answers = answers.Trim();

				if (withPageTitles && !string.IsNullOrEmpty(answers))
				{
					summaryBuilder.AddPageTitle(page.Title);
				}

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

		//public async Task<List<string>> GenerateQuestionAndAnswersListWithPageTitles(FormAnswers formAnswers, FormSchema formSchema)
		//{
		//	var summaryBuilder = new SummaryAnswerBuilder();
		//	var reducedAnswers = formAnswers.GetReducedAnswers(formSchema);

		//	if (!string.IsNullOrEmpty(formAnswers.CaseReference))
		//	{
		//		summaryBuilder.Add("Case Reference", formAnswers.CaseReference, Enum.EElementType.Textbox);
		//		summaryBuilder.AddBlankLine();
		//	}

		//	foreach (var page in formSchema.Pages.ToList())
		//	{
		//		var formSchemaQuestions = page.ValidatableElements
		//			.Where(_ => _ is not null)
		//			.ToList();

		//		if (formSchemaQuestions.Any())
		//		{
		//			summaryBuilder.AddPageTitle(page.Title);
		//		}

		//		if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug.Equals(page.PageSlug)).Select(p => p).Any())
		//			continue;

		//		formSchemaQuestions.ForEach(async question =>
		//		{
		//			var answer = await _elementMapper.GetAnswerStringValue(question, formAnswers);
		//			summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);

		//			summaryBuilder.AddBlankLine();
		//		});
		//	}

		//	return summaryBuilder.Build();
		//}

		public async Task<List<string>> GenerateQuestionAndAnswersListForPdf(FormAnswers formAnswers, FormSchema formSchema)
        {
            var summaryBuilder = new SummaryAnswerBuilder();
            var reducedAnswers = formAnswers.GetReducedAnswers(formSchema);

            if (!string.IsNullOrEmpty(formAnswers.CaseReference))
            {
                summaryBuilder.AddQuestion("Case Reference");
                summaryBuilder.AddAnswer(formAnswers.CaseReference);
                summaryBuilder.AddBlankLine();
            }

            foreach (var page in formSchema.Pages.ToList())
            {
                var formSchemaQuestions = page.ValidatableElements
                    .Where(_ => _ is not null)
                    .ToList();

                if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug.Equals(page.PageSlug)).Select(p => p).Any())
                    continue;

                foreach (var question in formSchemaQuestions)
                {
                    var answer = await _elementMapper.GetAnswerStringValue(question, formAnswers);

                    if (question.Type.Equals(EElementType.FileUpload) ||
                        question.Type.Equals(EElementType.MultipleFileUpload))
                    {
                        summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);
                    }
                    else
                    {
                        summaryBuilder.AddQuestion(question.GetLabelText(page.Title).Replace("(optional)", ""));
                        summaryBuilder.AddAnswer(answer);
                    }

                    summaryBuilder.AddBlankLine();
                }
            }

            return summaryBuilder.Build();
        }
    }
}