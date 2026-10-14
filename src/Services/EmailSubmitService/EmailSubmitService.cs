using File = StockportGovUK.NetStandard.Gateways.Models.FileManagement.File;

namespace form_builder.Services.EmailSubmitService;

public class EmailSubmitService(IEmailHelper emailHelper,
    IElementMapper elementMapper,
    IPageHelper pageHelper,
    IEmailProvider emailProvider,
    IReferenceNumberProvider referenceNumberProvider,
    IDocumentSummaryService documentSummaryService,
    IEnumerable<ITagParser> tagParsers,
    ILogger<EmailSubmitService> logger,
    IGateway gateway,
    IWebHostEnvironment hostingEnvironment,
    IOptions<PowerAutomateConfiguration> configuration)
    : IEmailSubmitService
{
    public async Task<string> EmailSubmission(MappingEntity data, string form, string cacheKey)
    {
        var reference = string.Empty;
        List<File> files = new();
        List<File> fileUploads = new();

        if (data.BaseForm.GenerateReferenceNumber)
        {
            data.FormAnswers.CaseReference = referenceNumberProvider.GetReference(data.BaseForm.ReferencePrefix);
            reference = data.FormAnswers.CaseReference;
            pageHelper.SaveCaseReference(cacheKey, reference);
        }

        var doc = documentSummaryService.GenerateDocument(
            new DocumentSummaryEntity
            {
                DocumentType = EDocumentType.Html,
                PreviousAnswers = data.FormAnswers,
                FormSchema = data.BaseForm
            });

        var email = await emailHelper.GetEmailInformation(form);
        var parsedSubjectInformation = new EmailConfiguration();

        try
        {
            var subjectInformation = JsonConvert.SerializeObject(email);
            subjectInformation = tagParsers.Aggregate(subjectInformation, (current, tagParser) => tagParser.ParseString(current, data.FormAnswers));
            parsedSubjectInformation = JsonConvert.DeserializeObject<EmailConfiguration>(subjectInformation);
        }
        catch (Exception ex)
        {
            parsedSubjectInformation.Subject = Regex.Replace(email.Subject, "{{.+}}", "");
            logger.LogInformation($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: '{email.Subject}' QuestionID contains a null value.", ex);
        }

        var subject = !string.IsNullOrEmpty(parsedSubjectInformation.Subject)
            ? parsedSubjectInformation.Subject
            : string.Empty;

        var body = string.IsNullOrWhiteSpace(email.Body)
            ? Encoding.Default.GetString(doc.Result)
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
                files = (List<File>)elementMapper.GetAnswerValue(element, data.FormAnswers).Result;
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
            var pdfdoc = documentSummaryService.GenerateDocument(
                new DocumentSummaryEntity
                {
                    DocumentType = EDocumentType.Pdf,
                    PreviousAnswers = data.FormAnswers,
                    FormSchema = data.BaseForm
                });

            emailMessage = fileUploads.Any() ?
                new EmailMessage(
                    subject,
                    body,
                    email.Sender,
                    string.Join(",", email.Recipient),
                    pdfdoc.Result,
                    $"{data.FormAnswers.CaseReference}_data.pdf",
                    fileUploads
                )
                : new EmailMessage(
                    subject,
                    body,
                    email.Sender,
                    string.Join(",", email.Recipient),
                    pdfdoc.Result,
                    $"{data.FormAnswers.CaseReference}_data.pdf"
                );
        }

        var result = emailProvider.SendEmail(emailMessage).Result;

        if (!result.Equals(HttpStatusCode.OK))
            throw new ApplicationException($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: threw an exception {result}");

        HttpResponseMessage response = new();

        try
        {
            var powerAutomateUrl = configuration.Value.BaseUrl;
            var powerAutomateDetails = new PowerAutomateDetails
            {
                FormName = form,
                Environment = hostingEnvironment.EnvironmentName
            };
            response = await gateway.PostAsync(powerAutomateUrl, powerAutomateDetails);

            if (!response.IsSuccessStatusCode) logger.LogError($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: " +
                                                                $"An unexpected error occurred writing to PowerAutomate {response}");
        }
        catch (Exception)
        {
            logger.LogError($"{nameof(EmailSubmitService)}::{nameof(EmailSubmission)}: " +
                             $"An unexpected error occurred writing to PowerAutomate {response}");
        }

        return reference;
    }
}