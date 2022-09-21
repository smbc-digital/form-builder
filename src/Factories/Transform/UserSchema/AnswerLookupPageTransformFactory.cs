using System.Text.Json;
using form_builder.Models;

namespace form_builder.Factories.Transform.UserSchema
{
    public class AnswerLookupPageTransformFactory : IUserPageTransformFactory
    {
        private static Page TransformPage(Page page, FormAnswers convertedAnswers)
        {
            
            var elements = page.Elements
                .Where(element => element.Lookup is not null &&
                       element.Lookup.StartsWith("#"));

            if (!elements.Any()) return page;

            foreach (var element in elements)
            {
                Answers answer = convertedAnswers.AllAnswers
                    .SingleOrDefault(answer => answer.QuestionId
                        .Equals(element.Lookup.TrimStart('#')));

                if (answer is not null && !string.IsNullOrEmpty((string)answer.Response))
                    element.Properties.Options
                        .AddRange(JsonSerializer.Deserialize<List<Option>>((string)answer.Response));
            }

            return page;
        }

        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
            => await Task.Run(() => TransformPage(page, convertedAnswers));
    }
}
