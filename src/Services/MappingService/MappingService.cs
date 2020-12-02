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
using static form_builder.Services.BookingService.BookingService;

namespace form_builder.Services.MappingService
{
    public interface IMappingService
    {
        Task<MappingEntity> Map(string sessionGuid, string form);
        Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel);
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

        public async Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel)
        {
            var baseForm = await _schemaFactory.Build(bookingElement.Properties.QuestionId);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(sessionGuid));

            return new BookingRequest
            {
                AppointmentId = bookingElement.Properties.AppointmentType,
                Customer = GetCustomerDetails(convertedAnswers, baseForm),
                StartDateTime = GetStartDateTime(bookingElement.Properties.QuestionId, viewModel)
            };
        }

        private DateTime GetStartDateTime(string formName, Dictionary<string, dynamic> viewModel)
        {
            var startDateTime = new DateTime();

            var bookingDateKey = $"{formName}{BookingConstants.APPOINTMENT_DATE}";
            var appointmentTimeKey = $"{formName}{BookingConstants.APPOINTMENT_TIME}";
            if (viewModel.ContainsKey(bookingDateKey))
            {
                DateTime day = DateTime.Parse(viewModel[bookingDateKey]);
                startDateTime = day;

                if (viewModel.ContainsKey(appointmentTimeKey))
                {
                    DateTime time = DateTime.Parse(viewModel[appointmentTimeKey]);
                    startDateTime = new DateTime(day.Year, day.Month, day.Day, time.Hour, time.Minute, time.Second);
                }
            }
            // ELSE : no actual booking date for request

            return startDateTime;
        }

        private Customer GetCustomerDetails(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;
            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .Where(x => !string.IsNullOrEmpty(x.Properties.TargetMapping) && x.Properties.TargetMapping.StartsWith("customer."))
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            var newCustomer = new Customer();
            data.TryGetValue("customer", out object customerObject);
            if (customerObject != null)
            {
                var customer = (IDictionary<String, Object>)customerObject;
                if (customer.ContainsKey("firstname") && customer.ContainsKey("lastname") && customer.ContainsKey("email"))
                {
                    newCustomer.Firstname = (string)customer["firstname"];
                    newCustomer.Lastname = (string)customer["lastname"];
                    newCustomer.Email = (string)customer["email"];

                    // Any additional data..?
                    customer.TryGetValue("address", out object address);
                    if (address != null)
                        newCustomer.Address = (StockportGovUK.NetStandard.Models.Addresses.Address)address;

                    if (customer.ContainsKey("phonenumber"))
                        newCustomer.PhoneNumber = (string)customer["phonenumber"];
                }
            }

            return newCustomer;
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
