using System.Text.RegularExpressions;
using form_builder.Helpers.PaymentHelpers;
using form_builder.Models;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class PaymentAmountTagParser : TagParser, ITagParser
    {
        private readonly IPaymentHelper _paymentHelper;

        public PaymentAmountTagParser(IEnumerable<IFormatter> formatters, IPaymentHelper paymentHelper) : base(formatters)
        {
            _paymentHelper = paymentHelper;
        }

        public Regex Regex => new Regex("(?<={{)PAYMENTAMOUNT.*?(?=}})", RegexOptions.Compiled);

        public async Task<Page> Parse(Page page, FormAnswers formAnswers, FormSchema baseForm = null)
        {
            var leadingParagraphRegexIsMatch = !string.IsNullOrEmpty(page.LeadingParagraph) && Regex.IsMatch(page.LeadingParagraph);
            var pageHasElementsMatchingRegex = page.Elements.Any(_ => _.Properties.Text is not null && Regex.IsMatch(_.Properties.Text));
            var pageHasConditionMatchingRegex = page.Behaviours is not null && page.Behaviours.Where(_ => _.Conditions is not null).Any(_ => _.Conditions.Any(_ => _.QuestionId is not null && Regex.IsMatch(_.QuestionId)));

            if (leadingParagraphRegexIsMatch || pageHasElementsMatchingRegex || pageHasConditionMatchingRegex)
            {
                var paymentAmount = string.Empty;

                if (!string.IsNullOrEmpty(formAnswers.PaymentAmount))
                {
                    paymentAmount = formAnswers.PaymentAmount;
                }
                else
                {
                    var paymentInformation = await _paymentHelper.GetFormPaymentInformation(formAnswers.FormName, formAnswers, baseForm);
                    paymentAmount = paymentInformation.Settings.Amount;
                }

                if (leadingParagraphRegexIsMatch)
                {
                    page.LeadingParagraph = Parse(page.LeadingParagraph, paymentAmount, Regex);
                }

                if (pageHasElementsMatchingRegex)
                {
                    page.Elements.Select(element =>
                    {
                        if (!string.IsNullOrEmpty(element.Properties?.Text))
                            element.Properties.Text = Parse(element.Properties.Text, paymentAmount, Regex);

                        return element;
                    }).ToList();
                }

                if (pageHasConditionMatchingRegex)
                {
                    foreach (var behaviour in page.Behaviours)
                    {
                        foreach (var condition in behaviour.Conditions.Where(condition => !string.IsNullOrEmpty(condition.QuestionId)))
                        {
                            condition.QuestionId = Parse(condition.QuestionId, paymentAmount, Regex);
                        }
                    }
                }
            }

            return page;
        }

        public string ParseString(string content, FormAnswers formAnswers)
        {
            if (Regex.IsMatch(content))
            {
                var paymentAmount = !string.IsNullOrEmpty(formAnswers.PaymentAmount)
                    ? formAnswers.PaymentAmount
                    : _paymentHelper.GetFormPaymentInformation(formAnswers.FormName, formAnswers, null).Result.Settings.Amount;

                return Parse(content, paymentAmount, Regex);
            }

            return content;
        }
    }
}
