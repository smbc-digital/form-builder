using System.Dynamic;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;
using File = StockportGovUK.NetStandard.Gateways.Models.FileManagement.File;

namespace form_builder.Services.MappingService
{
    public class MappingService : IMappingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly ISchemaFactory _schemaFactory;
        private readonly IPageHelper _pageHelper;
        private readonly IWebHostEnvironment _environment;
        private ILogger<MappingService> _logger;

        public MappingService(
            IDistributedCacheWrapper distributedCache,
            IPageHelper pageHelper,
            IElementMapper elementMapper,
            ISchemaFactory schemaFactory,
            IWebHostEnvironment environment,
            ILogger<MappingService> logger)
        {
            _distributedCache = distributedCache;
            _pageHelper = pageHelper;
            _elementMapper = elementMapper;
            _schemaFactory = schemaFactory;
            _environment = environment;
            _logger = logger;
        }

        public async Task<MappingEntity> Map(string cacheKey, string form)
        {
            var (convertedAnswers, baseForm) = await GetFormAnswers(form, cacheKey);

            return new MappingEntity
            {
                Data = await CreatePostData(convertedAnswers, baseForm),
                BaseForm = baseForm,
                FormAnswers = convertedAnswers
            };
        }

        public async Task<BookingRequest> MapBookingRequest(string cacheKey, IElement bookingElement, Dictionary<string, dynamic> viewModel, string form)
        {
            var (convertedAnswers, baseForm) = await GetFormAnswers(form, cacheKey);

            AppointmentType appointmentType = bookingElement.Properties.AppointmentTypes
                .GetAppointmentTypeForEnvironment(_environment.EnvironmentName);

            if (appointmentType.NeedsAppointmentIdMapping)
                MapAppointmentId(appointmentType, convertedAnswers);

            return new BookingRequest
            {
                AppointmentId = appointmentType.AppointmentId,
                Customer = await GetCustomerBookingDetails(convertedAnswers, baseForm, bookingElement),
                StartDateTime = GetStartDateTime(bookingElement.Properties.QuestionId, viewModel, form),
                OptionalResources = appointmentType.OptionalResources
            };
        }

        public void MapAppointmentId(AppointmentType appointmentType, FormAnswers answers)
        {
            Answers value = answers.Pages
                .SelectMany(page => page.Answers)
                .SingleOrDefault(answer => answer.QuestionId.Equals(appointmentType.AppointmentIdKey));

            appointmentType.AppointmentId = new Guid((string)value.Response);
        }

        private async Task<(FormAnswers convertedAnswers, FormSchema baseForm)> GetFormAnswers(string form, string cacheKey)
        {
            var baseForm = await _schemaFactory.Build(form);

            if (string.IsNullOrEmpty(cacheKey))
                throw new ApplicationException($"MappingService::GetFormAnswers:{cacheKey}, Session has expired");

            var sessionData = _distributedCache.GetString(cacheKey);
            if (sessionData is null)
                throw new ApplicationException($"MappingService::GetFormAnswer:{cacheKey}, Session data is null");

            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(sessionData);

            _logger.LogInformation($"{nameof(MappingService)}::{nameof(GetFormAnswers)}: " +
                                   $"Cached Form Answers before processing Reduced Answers - {JsonConvert.SerializeObject(convertedAnswers.Pages)}");

            convertedAnswers.Pages = convertedAnswers.GetReducedAnswers(baseForm);

            IEnumerable<string> visitedPageSlugs = convertedAnswers.Pages.Select(page => page.PageSlug);
            foreach (var pageSlug in visitedPageSlugs)
            {
                await _schemaFactory.TransformPage(baseForm.GetPage(_pageHelper, pageSlug, form), convertedAnswers);
            }

            convertedAnswers.FormName = form;
            if (convertedAnswers.Pages is null || !convertedAnswers.Pages.Any())
                _logger.LogInformation($"MappingService::GetFormAnswers:: Reduced Answers returned empty or null list, Creating submit data but no answers collected. Form {form}, Session {cacheKey}");

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

        private async Task<Customer> GetCustomerBookingDetails(FormAnswers formAnswers, FormSchema formSchema, IElement bookingElement)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;
            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .Where(x => !string.IsNullOrEmpty(x.Properties.TargetMapping)
                            && x.Properties.TargetMapping.StartsWith("customer.", StringComparison.OrdinalIgnoreCase)
                            && !x.Properties.TargetMapping.Equals("customer.address", StringComparison.OrdinalIgnoreCase))
                .ToList()
                .ForEach(async _ => data = await RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            if (!data.ContainsKey("customer"))
                throw new ApplicationException($"MappingService::GetCustomerDetails, Booking request form data for form {formSchema.BaseURL} does not contain required customer object");

            var customer = JsonConvert.DeserializeObject<Customer>(JsonConvert.SerializeObject(data["customer"]));
            if (string.IsNullOrEmpty(bookingElement.Properties.CustomerAddressId))
            {
                return customer;
            }

            var addressElement = formSchema.Pages.SelectMany(_ => _.Elements)
                .FirstOrDefault(_ =>
                    _.Properties.QuestionId is not null &&
                    _.Properties.QuestionId.Contains(bookingElement.Properties.CustomerAddressId));
            customer.Address = await _elementMapper.GetAnswerStringValue(addressElement, formAnswers);

            return customer;
        }

        private async Task<object> CreatePostData(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;

            var elements = formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .ToList();

            foreach (var element in elements)
            {
                data = await RecursiveCheckAndCreate(string.IsNullOrEmpty(element.Properties.TargetMapping) ? element.Properties.QuestionId : element.Properties.TargetMapping, element, formAnswers, data);
            }

            if (formAnswers.AdditionalFormData.Any())
                data = AddNonQuestionAnswers(data, formAnswers.AdditionalFormData);

            if (!string.IsNullOrEmpty(formAnswers.PaymentAmount))
                data = AddNonQuestionAnswers(data, new Dictionary<string, object> { { formSchema.PaymentAmountMapping, formAnswers.PaymentAmount } });

            if (!string.IsNullOrEmpty(formAnswers.CaseReference))
                data = AddNonQuestionAnswers(data, new Dictionary<string, object> { { "CaseReference", formAnswers.CaseReference } });

            return data;
        }

        private async Task<IDictionary<string, dynamic>> RecursiveCheckAndCreate(string targetMapping, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (element.Properties.IsDynamicallyGeneratedElement)
                return obj;

            if (splitTargets.Length.Equals(1))
            {

                if (element.Type.Equals(EElementType.FileUpload) || element.Type.Equals(EElementType.MultipleFileUpload))
                    return await CheckAndCreateForFileUpload(splitTargets[0], element, formAnswers, obj);

                if (element.Type.Equals(EElementType.AddAnother))
                    return await CheckAndCreateForAddAnother(splitTargets[0], element, formAnswers, obj);

                object answerValue = await _elementMapper.GetAnswerValue(element, formAnswers);

                if (answerValue is not null && obj.TryGetValue(splitTargets[0], out var objectValue))
                {
                    var combinedValue = $"{objectValue} {answerValue}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue.Trim());
                    return obj;
                }

                if (answerValue is not null)
                    obj.Add(splitTargets[0], answerValue);

                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = await RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, formAnswers, subObject as IDictionary<string, dynamic>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }

        private async Task<IDictionary<string, dynamic>> CheckAndCreateForAddAnother(string target, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            if (!formAnswers.FormData.Any() || !formAnswers.FormData.ContainsKey($"{AddAnotherConstants.IncrementKeyPrefix}{element.Properties.QuestionId}"))
                return obj;

            var savedIncrementValue = formAnswers.FormData[$"{AddAnotherConstants.IncrementKeyPrefix}{element.Properties.QuestionId}"].ToString();

            var numberOfIncrements = int.Parse(savedIncrementValue);
            var answers = new List<IDictionary<string, dynamic>>();

            for (var i = 1; i <= numberOfIncrements; i++)
            {
                var fieldsetAnswers = new Dictionary<string, dynamic>();
                foreach (var nestedElement in element.Properties.Elements)
                {
                    var incrementedElement = JsonConvert.DeserializeObject<IElement>(JsonConvert.SerializeObject(nestedElement));
                    incrementedElement.Properties.QuestionId = $"{nestedElement.Properties.QuestionId}_{i}_";
                    fieldsetAnswers = (Dictionary<string, dynamic>)await RecursiveCheckAndCreate(string.IsNullOrEmpty(nestedElement.Properties.TargetMapping) ? nestedElement.Properties.QuestionId : nestedElement.Properties.TargetMapping, incrementedElement, formAnswers, fieldsetAnswers);
                }

                answers.Add(fieldsetAnswers);
            }

            obj.Add(target, answers);

            return obj;
        }

        private async Task<IDictionary<string, dynamic>> CheckAndCreateForFileUpload(string target, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            object objectValue;
            var value = await _elementMapper.GetAnswerValue(element, formAnswers);

            if (obj.TryGetValue(target, out objectValue))
            {
                var files = (List<File>)objectValue;
                if (value is not null)
                {
                    obj.Remove(target);
                    files.AddRange((List<File>)value);
                    obj.Add(target, files);
                }

                return obj;
            }
            else
            {
                if (value is not null)
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
