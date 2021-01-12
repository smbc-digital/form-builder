using System.Collections.Generic;
using form_builder.Models;
using Microsoft.AspNetCore.Http;

namespace form_builder.Helpers.IncomingDataHelper
{
    public interface IIncomingDataHelper
    {
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData);
        Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, IQueryCollection queryCollection, FormAnswers formAnswers);
    }
}
