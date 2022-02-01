using form_builder.Models;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Factories.Transform.UserSchema
{
    public class OptionalIfTransformFactory : IUserPageTransformFactory
    {
        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
        {
            var optionalifElement = page.Elements.Where(_ => !string.IsNullOrEmpty(_.Properties.OptionalIfValue) || !string.IsNullOrEmpty(_.Properties.OptionalIfNotValue));

            if (optionalifElement.Any())
            {
                foreach (var e in optionalifElement)
                {
                    var userChoice = convertedAnswers.AllAnswers
                        .Where(x => x.QuestionId == e.Properties.OptionalIf)
                        .Select(x => x.Response)
                        .FirstOrDefault();

                    if (userChoice is not null)
                    {
                        if (userChoice.ToString() == e.Properties.OptionalIfValue || !string.IsNullOrEmpty(e.Properties.OptionalIfNotValue) && !(userChoice.ToString() == e.Properties.OptionalIfNotValue))
                            e.Properties.Optional = true;
                    }                                       
                }
            } 

            return await Task.FromResult(page);
        }
    }
}
