using System.Collections.Generic;
using System.Linq;
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

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            var leadingParagraphRegexIsMatch = !string.IsNullOrEmpty(page.LeadingParagraph) && Regex.IsMatch(page.LeadingParagraph);
            var pageHasElementsMatchingRegex = page.Elements.Any(_ => _.Properties.Text is not null && Regex.IsMatch(_.Properties.Text));

            if (leadingParagraphRegexIsMatch || pageHasElementsMatchingRegex)
            {
                var paymentAmount = !string.IsNullOrEmpty(formAnswers.PaymentAmount) 
                        ? formAnswers.PaymentAmount 
                        : _paymentHelper.GetFormPaymentInformation(formAnswers.FormName).Result.Settings.Amount;

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
            }

            return page;
        }

        public string ParseString(string content, FormAnswers formAnswers)
        {
            if (Regex.IsMatch(content))
            {
                var paymentAmount = !string.IsNullOrEmpty(formAnswers.PaymentAmount)
                    ? formAnswers.PaymentAmount
                    : _paymentHelper.GetFormPaymentInformation(formAnswers.FormName).Result.Settings.Amount;

                return Parse(content, paymentAmount, Regex);
            }

            return content;
        }
    }
}
