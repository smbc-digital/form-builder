using System.Collections.Generic;
using form_builder.Models;
using form_builder.Models.Actions;
using Microsoft.AspNetCore.Http;

namespace form_builder.Helpers.IncomingDataHelper
{
    public interface IIncomingDataHelper
    {
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData);
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, IQueryCollection queryCollection, FormAnswers formAnswers);
        Dictionary<string, dynamic> AddIncomingFormDataValues(IAction action, IQueryCollection queryCollection, FormAnswers formAnswers);
    }
}
