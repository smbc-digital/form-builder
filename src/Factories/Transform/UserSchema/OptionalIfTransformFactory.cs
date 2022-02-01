using System.Linq;
using System.Threading.Tasks;
using form_builder.Models;

namespace form_builder.Factories.Transform.UserSchema
{
    public class OptionalIfTransformFactory : IUserPageTransformFactory
    {
        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
        {
            var optionalIfElements = page.Elements
                .Where(element => !string.IsNullOrEmpty(element.Properties.OptionalIfValue) 
                || !string.IsNullOrEmpty(element.Properties.OptionalIfNotValue));

            if (optionalIfElements.Any())
            {
                foreach (var element in optionalIfElements)
                {
                    var userChoice = convertedAnswers.AllAnswers
                        .Where(answer => answer.QuestionId.Equals(element.Properties.OptionalIfQuestionId))
                        .FirstOrDefault(answer => answer.Response);

                    if (userChoice is not null)
                    {
                        if (userChoice.ToString().Equals(element.Properties.OptionalIfValue) 
                            || !string.IsNullOrEmpty(element.Properties.OptionalIfNotValue) 
                            && !(userChoice.ToString().Equals(element.Properties.OptionalIfNotValue)))
                            element.Properties.Optional = true;
                    }                                       
                }
            } 

            return await Task.FromResult(page);
        }
    }
}
