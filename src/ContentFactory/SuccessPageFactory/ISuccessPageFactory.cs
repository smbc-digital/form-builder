using form_builder.Enum;
using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.ContentFactory.SuccessPageFactory;

public interface ISuccessPageFactory
{
    Task<SuccessPageEntity> Build(string form, FormSchema baseForm, string cacheKey, FormAnswers formAnswers, EBehaviourType behaviourType);
    Task<SuccessPageEntity> BuildBooking(string form, FormSchema baseForm, string cacheKey, FormAnswers formAnswers);
}