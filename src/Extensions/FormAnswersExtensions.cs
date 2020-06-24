﻿﻿using form_builder.Enum;
using form_builder.Models;
using System.Collections.Generic;
using System.Linq;

namespace form_builder.Extensions
{
    public static class FormAnswersExtensions
    {
        public static List<PageAnswers> GetReducedAnswers(this FormAnswers answer, FormSchema schema)
            => RecursivelyReduceAnswers(
                answer.Pages,
                answer.Pages.SelectMany(_ => _.Answers).ToDictionary(x => x.QuestionId, x => x.Response),
                schema.Pages,
                schema.StartPageSlug,
                new List<PageAnswers>());

        private static List<PageAnswers> RecursivelyReduceAnswers(
            List<PageAnswers> answers,
            Dictionary<string, object> answersDictionary,
            List<Page> schema,
            string currentPageSlug,
            List<PageAnswers> reducedAnswers)
        {
            var currentAnswer = answers.Find(_ => _.PageSlug.Equals(currentPageSlug));
            if (currentAnswer == null)
                return reducedAnswers;

            var currentSchema = schema.Find(_ => _.PageSlug.Equals(currentPageSlug));
            if (currentSchema == null)
                return reducedAnswers;

            reducedAnswers.Add(currentAnswer);

            var behaviour = currentSchema.GetNextPage(answersDictionary);
            if (behaviour.BehaviourType != EBehaviourType.GoToPage)
                return reducedAnswers;

            return RecursivelyReduceAnswers(answers, answersDictionary, schema, behaviour.PageSlug, reducedAnswers);
        }
    }
}