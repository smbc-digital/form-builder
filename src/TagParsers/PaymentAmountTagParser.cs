using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder.TagParsers.Formatters;

namespace form_builder.TagParsers
{
    public class PaymentAmountTagParser : TagParser, ITagParser
    {
        private readonly IPayService _payService;

        public PaymentAmountTagParser(IEnumerable<IFormatter> formatters, IPayService payService) : base(formatters) 
        {
            _payService = payService;
        }

        public Regex Regex => new Regex("(?<={{)PAYMENTAMOUNT.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var leadingParagraphRegexIsMatch = !string.IsNullOrEmpty(page.LeadingParagraph) && Regex.IsMatch(page.LeadingParagraph);
            var pageHasElementsMatchingRegex = page.Elements.Any(_ => _.Properties.Text != null && Regex.IsMatch(_.Properties.Text));
            var pageHasConditionMatchingRegex = page.Behaviours.Any(_ => _.Conditions.Any(_ => _.QuestionId is not null && Regex.IsMatch(_.QuestionId)));

            if (leadingParagraphRegexIsMatch || pageHasElementsMatchingRegex || pageHasConditionMatchingRegex)
            {
                var paymentAmount = !string.IsNullOrEmpty(formAnswers.PaymentAmount) 
                        ? formAnswers.PaymentAmount 
                        : _payService.GetFormPaymentInformation(formAnswers.FormName).Result.Settings.Amount;

                if (leadingParagraphRegexIsMatch)
                {
                    page.LeadingParagraph = Parse(page.LeadingParagraph, paymentAmount, Regex);
                }

                if (pageHasElementsMatchingRegex)
                {
                    page.Elements.Select((element) =>
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
                        foreach (var condition in behaviour.Conditions)
                        {
                            if (!string.IsNullOrEmpty(condition.QuestionId))
                                condition.QuestionId = Parse(condition.QuestionId, paymentAmount, Regex);
                        }
                    }
                }
            }

            return page;
        }
    }
}
