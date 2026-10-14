namespace form_builder.Helpers.DocumentCreation;

public class DocumentCreationHelper(IElementMapper elementMapper) : IDocumentCreationHelper
{
    public async Task<List<string>> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema)
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

            if (!formSchemaQuestions.Any() || !reducedAnswers.Where(p => p.PageSlug.Equals(page.PageSlug)).Select(p => p).Any())
                continue;

            formSchemaQuestions.ForEach(async question =>
            {
                var answer = await elementMapper.GetAnswerStringValue(question, formAnswers);
                summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);

                summaryBuilder.AddBlankLine();
            });
        }

        return summaryBuilder.Build();
    }

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
                var answer = await elementMapper.GetAnswerStringValue(question, formAnswers);

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