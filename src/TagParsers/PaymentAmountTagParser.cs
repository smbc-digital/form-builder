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

        public Regex Regex => new Regex("(?<={{)PaymentAmount.*?(?=}})", RegexOptions.Compiled);

        public Page Parse(Page page, FormAnswers formAnswers)
        {
            if (page.Elements.Any(_ => _.Properties.Text != null && Regex.IsMatch(_.Properties.Text)))
            {
                var paymentInfo = _payService.GetFormPaymentInformation(formAnswers.FormName, page).Result;

                page.Elements.Select((element) =>
                {
                    if (!string.IsNullOrEmpty(element.Properties?.Text))
                        element.Properties.Text = Parse(element.Properties.Text, paymentInfo.Settings.Amount, Regex);

                    return element;
                }).ToList();
            }

            return page;
        }
    }
}
