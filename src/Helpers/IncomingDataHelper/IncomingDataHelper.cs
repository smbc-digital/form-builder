using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using form_builder.Enum;
using form_builder.Models;
using Microsoft.AspNetCore.Http;

namespace form_builder.Helpers.IncomingDataHelper
{
    public class IncomingDataHelper : IIncomingDataHelper
    {
        public Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, Dictionary<string, dynamic> formData)
        {
            page.IncomingValues.Where(_ => _.HttpActionType.Equals(EHttpActionType.Post)).ToList().ForEach(_ =>
            {
                var containsValue = formData.ContainsKey(_.Name);

                if (!_.Optional && !containsValue)
                    throw new Exception($"IncomingDataHelper::AddIncomingFormDataValues, FormData does not contain {_.Name} required value");

                if (!containsValue) return;

                formData = RecursiveCheckAndCreate(_.QuestionId, formData[_.Name], formData);
                formData.Remove(_.Name);
            });

            return formData;
        }

        public Dictionary<string, dynamic> AddIncomingFormDataValues(Page page, IQueryCollection queryCollection, FormAnswers formAnswers)
        {
            var formData = new Dictionary<string, dynamic>();
            var queryData = queryCollection.ToDictionary(x => x.Key, x => (dynamic)x.Value);
            page.IncomingValues.Where(_ => _.HttpActionType.Equals(EHttpActionType.Get)).ToList().ForEach(_ =>
            {
                var containsValue = queryData.ContainsKey(_.Name);

                if (!containsValue && formAnswers.AdditionalFormData.ContainsKey(_.QuestionId))
                    return;

                if (!_.Optional && !containsValue)
                    throw new Exception($"IncomingDataHelper::AddIncomingFormDataValues, FormData does not contain {_.Name} required value");

                if (!containsValue) return;

                dynamic value = queryCollection[_.Name];
                if (_.Base64Encoded)
                    value = Encoding.UTF8.GetString(Convert.FromBase64String(value));

                formData = RecursiveCheckAndCreate(_.QuestionId, value, formData);
            });

            return formData;
        }

        private IDictionary<string, dynamic> RecursiveCheckAndCreate(string targetMapping, string value, IDictionary<string, dynamic> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length.Equals(1))
            {
                obj.Add(splitTargets[0], value);
                return obj;
            }

            if (!obj.TryGetValue(splitTargets[0], out object subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", string.Empty), value, subObject as IDictionary<string, dynamic>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }
    }
}
