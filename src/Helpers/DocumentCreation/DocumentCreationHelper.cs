using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Builders.Document;
using form_builder.Enum;
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

            formSchema.Pages.ForEach(page =>
            {
                var formSchemaQuestions = page.ValidatableElements
                    .Where(_ => _ != null)
                    .ToList();

                formSchemaQuestions.ForEach(async question =>
                {
                    var answer = await _elementMapper.GetAnswerStringValue(question, formAnswers);
                    summaryBuilder.Add(question.GetLabelText(page.Title), answer, question.Type);

                    summaryBuilder.AddBlankLine();
                });
            });

            return summaryBuilder.Build();
        }
    }
}