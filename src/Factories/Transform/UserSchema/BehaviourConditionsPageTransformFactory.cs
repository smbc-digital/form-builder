using System.Linq;
using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Models;

namespace form_builder.Factories.Transform.UserSchema
{
    public class BehaviourConditionsPageTransformFactory : IUserPageTransformFactory
    {
        public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
        {
            if (page.Behaviours is not null && page.Behaviours.Any() && page.Behaviours.Any(_ => _.Conditions.Any()))
            {
                foreach (var behaviour in page.Behaviours.Where(_ => _.Conditions.Any()))
                {
                    foreach (var condition in behaviour.Conditions)
                    {
                        switch (condition.ConditionType)
                        {
                            case ECondition.PaymentAmountEqualTo:
                                condition.QuestionId = "{{PAYMENTAMOUNT}}";
                                break;
                        }
                    }
                }
            }

            return page;
        }
    }
}
