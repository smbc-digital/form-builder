﻿using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.MappingService;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Services.MappingService.Entities;

namespace form_builder.Services.EmailSubmitService
{
    public class EmailSubmitService : IEmailSubmitService
    {
        private readonly IMappingService _mappingService;
        private readonly IEmailHelper _emailHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IEmailProvider _emailProvider;
        private readonly IPageHelper _pageHelper;
        private readonly IDocumentSummaryService _documentSummaryService;
        private readonly IReferenceNumberProvider _referenceNumberProvider;

        public EmailSubmitService(
            IMappingService mappingService,
            IEmailHelper emailHelper,
            ISessionHelper sessionHelper,
            IPageHelper pageHelper,
            IEmailProvider emailProvider,
            IReferenceNumberProvider referenceNumberProvider,
            IDocumentSummaryService documentSummaryService
            )
        {
            _mappingService = mappingService;
            _emailHelper = emailHelper;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _emailProvider = emailProvider;
            _referenceNumberProvider = referenceNumberProvider;
            _documentSummaryService = documentSummaryService;
        }
            

        public async Task<string> EmailSubmission(MappingEntity data, string form, string sessionGuid)
        {
            var reference = string.Empty;

            if (data.BaseForm.GenerateReferenceNumber)
            {
                data.FormAnswers.CaseReference = _referenceNumberProvider.GetReference(data.BaseForm.ReferencePrefix);
                reference = data.FormAnswers.CaseReference;
                _pageHelper.SaveCaseReference(sessionGuid, reference);
                data = await _mappingService.Map(sessionGuid, form);
            }

            var doc = _documentSummaryService.GenerateDocument(
               new DocumentSummaryEntity
               {
                   DocumentType = EDocumentType.Html,
                   PreviousAnswers = data.FormAnswers,
                   FormSchema = data.BaseForm
               });

            var body = System.Text.Encoding.Default.GetString(doc.Result);

            var email = _emailHelper.GetEmailInformation(form).Result;
            var emailMessage = new EmailMessage
                (email.Subject, body,
                "noreply@stockport.gov.uk",
                string.Join(",", email.To.ToArray())
                );

            var result = _emailProvider.SendEmail(emailMessage).Result;

            if (result != System.Net.HttpStatusCode.OK)
                throw new ApplicationException($"Error sending email {result}");

            return reference;
        }
    }
}
