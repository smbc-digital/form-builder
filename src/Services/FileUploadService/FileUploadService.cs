using System.Collections.Generic;
using System.Linq;
using System.Net;
using form_builder.Configuration;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.FileUploadService
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ISessionHelper _sessionHelper;

        public FileUploadService(IDistributedCacheWrapper distributedCache,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ISessionHelper sessionHelper)
        {
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _sessionHelper = sessionHelper;
        }

        public Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload)
        {
            fileUpload.Where(_ => _ != null)
                .ToList()
                .ForEach((file) =>
                {
                    viewModel.Add(file.QuestionId, new DocumentModel { Content = file.Base64EncodedContent, FileSize = file.Length });
                });

            return viewModel;
        }

        public List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files)
        {
            files.ToList().ForEach(file =>
            {
                var key = $"{ file.QuestionId}-{_sessionHelper.GetSessionGuid()}";
                _distributedCache.SetStringAsync($"file-{key}", file.Base64EncodedContent, _distributedCacheExpirationConfiguration.FileUpload);
                var model = new FileUploadModel
                {
                    Key = $"file-{key}",
                    TrustedOriginalFileName = WebUtility.HtmlEncode(file.UntrustedOriginalFileName),
                    UntrustedOriginalFileName = file.UntrustedOriginalFileName
                };

                if (answers.Exists(_ => _.QuestionId == file.QuestionId))
                {
                    var fileUploadAnswer = answers.FirstOrDefault(_ => _.QuestionId == file.QuestionId);
                    if (fileUploadAnswer != null)
                        fileUploadAnswer.Response = model;
                } 
                else
                {
                    answers.Add(new Answers { QuestionId = file.QuestionId, Response = JsonConvert.SerializeObject(model) });
                }
            });

            return answers;
        }
    }
}
