﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.FileManagement;

namespace form_builder.Services.MappingService
{
    public interface IMappingService
    {
        Task<MappingEntity> Map(string sessionGuid, string form);
        Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form);
    }

    public class MappingService : IMappingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public MappingService(IDistributedCacheWrapper distributedCache,
            IElementMapper elementMapper,
            ISchemaFactory schemaFactory,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _distributedCache = distributedCache;
            _elementMapper = elementMapper;
            _schemaFactory = schemaFactory;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<MappingEntity> Map(string sessionGuid, string form)
        {
            var baseForm = await _schemaFactory.Build(form);
            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            convertedAnswers.FormName = form;
            convertedAnswers.Pages = convertedAnswers.GetReducedAnswers(baseForm);

            return new MappingEntity
            {
                Data = CreatePostData(convertedAnswers, baseForm),
                BaseForm = baseForm,
                FormAnswers = convertedAnswers,
            };
        }

        public async Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form)
        {
            var baseForm = await _schemaFactory.Build(form);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(sessionGuid));
            convertedAnswers.Pages = convertedAnswers.GetReducedAnswers(baseForm);
            convertedAnswers.FormName = form;

            return new BookingRequest
            {
                AppointmentId = bookingElement.Properties.AppointmentType,
                Customer = GetCustomerDetails(convertedAnswers, baseForm),
                StartDateTime = GetStartDateTime(bookingElement.Properties.QuestionId, viewModel, form)
            };
        }

        private DateTime GetStartDateTime(string questionID, Dictionary<string, dynamic> viewModel, string form)
        {
            var bookingDateKey = $"{questionID}-{BookingConstants.APPOINTMENT_DATE}";
            var appointmentTimeKey = $"{questionID}-{BookingConstants.APPOINTMENT_START_TIME}";

            if (!viewModel.ContainsKey(bookingDateKey))
                throw new ApplicationException($"MappingService::GetStartDateTime, Booking request viewmodel for form {form} does not contain required booking start date");

            if (!viewModel.ContainsKey(appointmentTimeKey))
                throw new ApplicationException($"MappingService::GetStartDateTime, Booking request viewmodel for form {form} does not contain required booking start time");

            DateTime startDateTime = DateTime.Parse(viewModel[bookingDateKey]);
            DateTime time = DateTime.Parse(viewModel[appointmentTimeKey]);

            return new DateTime(startDateTime.Year, startDateTime.Month, startDateTime.Day, time.Hour, time.Minute, time.Second);
        }

        private Customer GetCustomerDetails(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;
            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .Where(x => !string.IsNullOrEmpty(x.Properties.TargetMapping) && x.Properties.TargetMapping.ToLower().StartsWith("customer."))
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            if(!data.ContainsKey("customer"))
                throw new ApplicationException($"MappingService::GetCustomerDetails, Booking request form data for form {formSchema.BaseURL} does not contain required customer object");

            return JsonConvert.DeserializeObject<Customer>(JsonConvert.SerializeObject(data["customer"]));
        }

        private object CreatePostData(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;

            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            if (formAnswers.AdditionalFormData.Any())
                data = AddNonQuestionAnswers(data, formAnswers.AdditionalFormData);

            return data;
        }

        private IDictionary<string, dynamic> RecursiveCheckAndCreate(string targetMapping, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
            {
                if (element.Type == EElementType.FileUpload || element.Type == EElementType.MultipleFileUpload)
                    return CheckAndCreateForFileUpload(splitTargets[0], element, formAnswers, obj);

                object answerValue = _elementMapper.GetAnswerValue(element, formAnswers);
                if (answerValue != null && obj.TryGetValue(splitTargets[0], out var objectValue))
                {
                    var combinedValue = $"{objectValue} {answerValue}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue.Trim());
                    return obj;
                }

                if (answerValue != null)
                    obj.Add(splitTargets[0], answerValue);

                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, formAnswers, subObject as IDictionary<string, dynamic>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }

        private IDictionary<string, dynamic> CheckAndCreateForFileUpload(string target, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            object objectValue;
            var value = _elementMapper.GetAnswerValue(element, formAnswers);

            if (obj.TryGetValue(target, out objectValue))
            {
                var files = (List<File>)objectValue;
                if (value != null)
                {
                    obj.Remove(target);
                    files.AddRange((List<File>)value);
                    obj.Add(target, files);
                }

                return obj;
            }
            else
            {
                if (value != null)
                {
                    obj.Add(target, (List<File>)value);
                }
            }

            return obj;
        }

        private IDictionary<string, dynamic> AddNonQuestionAnswers(IDictionary<string, dynamic> currentData, Dictionary<string, object> newData)
        {
            newData.ToList().ForEach(x => currentData.Add(x.Key, x.Value));
            return currentData;
        }
    }
}
