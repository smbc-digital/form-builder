using System;
using System.Text.RegularExpressions;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.TagParsers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using File = StockportGovUK.NetStandard.Gateways.Models.FileManagement.File;

namespace form_builder.Services.EmailSubmitService;

public class EmailSubmitService : IEmailSubmitService
{
    private readonly IMappingService _mappingService;
    private readonly IElementMapper _elementMapper;
    private readonly IEmailHelper _emailHelper;
    private readonly ISessionHelper _sessionHelper;
    private readonly IEmailProvider _emailProvider;
    private readonly IPageHelper _pageHelper;
    private readonly IDocumentSummaryService _documentSummaryService;
    private readonly IReferenceNumberProvider _referenceNumberProvider;
    private readonly IEnumerable<ITagParser> _tagParsers;
    private readonly ILogger<EmailSubmitService> _logger;
    private readonly IGateway _gateway;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private IOptions<PowerAutomateConfiguration> _configuration;

    public EmailSubmitService(
        IMappingService mappingService,
        IEmailHelper emailHelper,
        IElementMapper elementMapper,
        ISessionHelper sessionHelper,
        IPageHelper pageHelper,
        IEmailProvider emailProvider,
        IReferenceNumberProvider referenceNumberProvider,
        IDocumentSummaryService documentSummaryService,
        IEnumerable<ITagParser> tagParsers,
        ILogger<EmailSubmitService> logger,
        IGateway gateway,
        IWebHostEnvironment hostingEnvironment,
        IOptions<PowerAutomateConfiguration> configuration
    )
    {
        _mappingService = mappingService;
        _emailHelper = emailHelper;
        _elementMapper = elementMapper;
        _pageHelper = pageHelper;
        _sessionHelper = sessionHelper;
        _emailProvider = emailProvider;
        _referenceNumberProvider = referenceNumberProvider;
        _documentSummaryService = documentSummaryService;
        _tagParsers = tagParsers;
        _logger = logger;
        _gateway = gateway;
        _hostingEnvironment = hostingEnvironment;
        _configuration = configuration;
    }

    public async Task<string> EmailSubmission(MappingEntity data, string form, string cacheKey)
    {
        var reference = string.Empty;
        List<File> files = new();
        List<File> fileUploads = new();

        if (data.BaseForm.GenerateReferenceNumber)
        {
            data.FormAnswers.CaseReference = _referenceNumberProvider.GetReference(data.BaseForm.ReferencePrefix);
            reference = data.FormAnswers.CaseReference;
            _pageHelper.SaveCaseReference(cacheKey, reference);
        }

        var doc = _documentSummaryService.GenerateDocument(
            new DocumentSummaryEntity
            {
                DocumentType = EDocumentType.Html,
                PreviousAnswers = data.FormAnswers,
                FormSchema = data.BaseForm
            });

        var email = await _emailHelper.GetEmailInformation(form);
        var parsedSubjectInformation = new EmailConfiguration();

        try
        {
            var subjectInformation = JsonConvert.SerializeObject(email);
            subjectInformation = _tagParsers.Aggregate(subjectInformation, (current, tagParser) => tagParser.ParseString(current, data.FormAnswers));
            parsedSubjectInformation = JsonConvert.DeserializeObject<EmailConfiguration>(subjectInformation);
        }
        catch (Exception ex)
        {
            parsedSubjectInformation.Subject = Regex.Replace(email.Subject, "{{.+}}", "");
            _logger.LogInformation($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: '{email.Subject}' QuestionID contains a null value.", ex);
        }

        var subject = !string.IsNullOrEmpty(parsedSubjectInformation.Subject)
            ? parsedSubjectInformation.Subject
            : string.Empty;

        var body = string.IsNullOrWhiteSpace(email.Body)
            ? System.Text.Encoding.Default.GetString(doc.Result)
            : $"<p>{email.Body}</p>";

        var emailMessage = new EmailMessage(
            subject,
            body,
            email.Sender,
            string.Join(",", email.Recipient)
        );

        bool isFileUpload = data.FormAnswers.AllAnswers
            .Any(_ => _.QuestionId.Contains(FileUploadConstants.SUFFIX));

        if (isFileUpload)
        {
            var fileElements = data.BaseForm.Pages
                .SelectMany(_ => _.Elements)
                .Where(_ => _.Type.Equals(EElementType.FileUpload) || _.Type.Equals(EElementType.MultipleFileUpload));

            foreach (var element in fileElements)
            {
                files = (List<File>)_elementMapper.GetAnswerValue(element, data.FormAnswers).Result;
                foreach (File file in files ?? new List<File>())
                    fileUploads.Add(file);
            }

            if (fileUploads.Any())
            {
                emailMessage = new EmailMessage(
                    subject,
                    body,
                    email.Sender,
                    string.Join(",", email.Recipient),
                    fileUploads
                );
            }
        }

        if (email.AttachPdf)
        {
            var pdfdoc = _documentSummaryService.GenerateDocument(
                new DocumentSummaryEntity
                {
                    DocumentType = EDocumentType.Pdf,
                    PreviousAnswers = data.FormAnswers,
                    FormSchema = data.BaseForm
                });

            emailMessage = fileUploads.Any() ?
                emailMessage = new EmailMessage(
                    subject,
                    body,
                    email.Sender,
                    string.Join(",", email.Recipient),
                    pdfdoc.Result,
                    $"{data.FormAnswers.CaseReference}_data.pdf",
                    fileUploads
                )
                : emailMessage = new EmailMessage(
                    subject,
                    body,
                    email.Sender,
                    string.Join(",", email.Recipient),
                    pdfdoc.Result,
                    $"{data.FormAnswers.CaseReference}_data.pdf"
                );
        }

        var result = _emailProvider.SendEmail(emailMessage).Result;

        if (!result.Equals(System.Net.HttpStatusCode.OK))
            throw new ApplicationException($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: threw an exception {result}");

        HttpResponseMessage response = new();

        try
        {
            FormSchema baseForm = new FormSchema();
            var powerAutomateUrl = _configuration.Value.BaseUrl;
            var powerAutomateDetails = new PowerAutomateDetails()
            {
                FormName = form,
                Environment = _hostingEnvironment.EnvironmentName
            };
            response = await _gateway.PostAsync(powerAutomateUrl, powerAutomateDetails);

            if (!response.IsSuccessStatusCode) _logger.LogError($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: " +
                                                                $"An unexpected error occurred writting to PowerAutomate {response}");
        }
        catch (Exception)
        {
            _logger.LogError($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: " +
                             $"An unexpected error occurred writting to PowerAutomate {response}");
        }

        return reference;
    }
}