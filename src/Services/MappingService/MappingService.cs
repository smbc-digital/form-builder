using System;
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Booking.Request;
using StockportGovUK.NetStandard.Models.FileManagement;

namespace form_builder.Services.MappingService
{
    public class MappingService : IMappingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly IWebHostEnvironment _environment;
        private ILogger<MappingService> _logger;

        public MappingService(IDistributedCacheWrapper distributedCache,
            IElementMapper elementMapper,
            ISchemaFactory schemaFactory,
            IWebHostEnvironment environment,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ILogger<MappingService> logger)
        {
            _distributedCache = distributedCache;
            _elementMapper = elementMapper;
            _schemaFactory = schemaFactory;
            _environment = environment;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _logger = logger;
        }

        public async Task<MappingEntity> Map(string sessionGuid, string form)
        {
            var (convertedAnswers, baseForm) = await GetFormAnswers(form, sessionGuid);

            return new MappingEntity
            {
                Data = CreatePostData(convertedAnswers, baseForm),
                BaseForm = baseForm,
                FormAnswers = convertedAnswers
            };
        }

        public async Task<BookingRequest> MapBookingRequest(string sessionGuid, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form)
        {
            var (convertedAnswers, baseForm) = await GetFormAnswers(form, sessionGuid);
            
            var appointmentType = bookingElement.Properties.AppointmentTypes.GetAppointmentTypeForEnvironment(_environment.EnvironmentName);

            return new BookingRequest
            {
                AppointmentId = appointmentType.AppointmentId,
                Customer = GetCustomerBookingDetails(convertedAnswers, baseForm, bookingElement),
                StartDateTime = GetStartDateTime(bookingElement.Properties.QuestionId, viewModel, form),
                OptionalResources = appointmentType.OptionalResources
            };
        }

        private async Task<(FormAnswers convertedAnswers, FormSchema baseForm)> GetFormAnswers(string form, string sessionGuid)
        {
            var baseForm = await _schemaFactory.Build(form);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(sessionGuid));
            convertedAnswers.Pages = convertedAnswers.GetReducedAnswers(baseForm);
            convertedAnswers.FormName = form;

            if(convertedAnswers.Pages == null || !convertedAnswers.Pages.Any())
                _logger.LogWarning($"MappingService::GetFormAnswers, Reduced Answers returned empty or null list, Creating submit data but no answers collected. Form {form}, Session {sessionGuid}");

            return (convertedAnswers, baseForm);
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

        private Customer GetCustomerBookingDetails(FormAnswers formAnswers, FormSchema formSchema, IElement bookingElement)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;
            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .Where(x => !string.IsNullOrEmpty(x.Properties.TargetMapping) 
                            && x.Properties.TargetMapping.ToLower().StartsWith("customer.") 
                            && !x.Properties.TargetMapping.ToLower().Equals("customer.address"))
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            if (!data.ContainsKey("customer"))
                throw new ApplicationException($"MappingService::GetCustomerDetails, Booking request form data for form {formSchema.BaseURL} does not contain required customer object");

            var customer = JsonConvert.DeserializeObject<Customer>(JsonConvert.SerializeObject(data["customer"]));
            if (string.IsNullOrEmpty(bookingElement.Properties.CustomerAddressId))
            {
                return customer;
            }

            var addressElement = formSchema.Pages.SelectMany(_ => _.Elements)
                .FirstOrDefault(_ =>
                    _.Properties.QuestionId != null &&
                    _.Properties.QuestionId.Contains(bookingElement.Properties.CustomerAddressId));
            customer.Address = _elementMapper.GetAnswerStringValue(addressElement, formAnswers);

            return customer;
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
