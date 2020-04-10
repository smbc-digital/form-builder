using form_builder.Mappers;
using form_builder.Models;
using form_builder.Builders.Document;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Helpers.DocumentCreation
{
    public interface IDocumentCreationHelper
    {
        List<string> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema);
    }

    public class DocumentCreationHelper : IDocumentCreationHelper
    {
        private readonly IElementMapper _elementMapper;
        public DocumentCreationHelper(IElementMapper elementMapper)
        {
            _elementMapper = elementMapper;
        }
        
        public List<string> GenerateQuestionAndAnswersList(FormAnswers formAnswers, FormSchema formSchema)
        {
            var summaryBuilder = new SummaryAnswerBuilder();

            var formSchemaQuestions = formSchema.Pages.ToList()
                .Where(_ => _.Elements != null)
                .SelectMany(_ => _.ValidatableElements)
                .ToList();

            formSchemaQuestions.ForEach((question) => {
                var answer = _elementMapper.GetAnswerStringValue(question, formAnswers);

                summaryBuilder.Add(question.GetLabelText(), answer, question.Type);
            });

            return summaryBuilder.Build();
        }
    }
}