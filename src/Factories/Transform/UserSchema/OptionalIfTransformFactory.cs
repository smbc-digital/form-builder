using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Conditions;
using form_builder.Models;
using form_builder.Models.Elements;

namespace form_builder.Factories.Transform.UserSchema
{
    public class OptionalIfTransformFactory : IUserPageTransformFactory
    {
        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
        { 
            IEnumerable<IElement> elements = page.ValidatableElements
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIf?.QuestionId));

            if (!elements.Any())
                return await Task.FromResult(page);

            foreach (IElement element in elements)
            {
                var userChoice = convertedAnswers.AllAnswers
                    .FirstOrDefault(answer => answer.QuestionId
                    .Equals(element.Properties.OptionalIf.QuestionId));

                var questionsAnswers = new Dictionary<string, dynamic>
                {
                    { userChoice.QuestionId, userChoice.Response }
                };

                var conditionValidator = new ConditionValidator();
                element.Properties.Optional = conditionValidator.IsValid(element.Properties.OptionalIf, questionsAnswers);
            }                                    

            return await Task.FromResult(page);
        }
    }
}
