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
                .GroupBy(_ => _.QuestionId)
                .ToList()
                .ForEach((group) =>
                {
                    viewModel.Add(group.Key, group.Select(_ => new DocumentModel { Content =_.Base64EncodedContent, FileSize = _.Length }).ToList());
                });

            return viewModel;
        }

        public List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files)
        {
            files.GroupBy(_ => _.QuestionId).ToList().ForEach(file =>
            {
                var key = $"{ file.Key}-{_sessionHelper.GetSessionGuid()}";
                var fileContent = file.Select(_ => _.Base64EncodedContent);
                _distributedCache.SetStringAsync($"file-{key}", JsonConvert.SerializeObject(fileContent), _distributedCacheExpirationConfiguration.FileUpload);
                
                var fileUploadModel = file.Select(_ => new FileUploadModel
                {
                    Key = $"file-{key}",
                    TrustedOriginalFileName = WebUtility.HtmlEncode(_.UntrustedOriginalFileName),
                    UntrustedOriginalFileName = _.UntrustedOriginalFileName
                });               

                if (answers.Exists(_ => _.QuestionId == file.Key))
                {
                    var fileUploadAnswer = answers.FirstOrDefault(_ => _.QuestionId == file.Key);
                    if (fileUploadAnswer != null)
                        fileUploadAnswer.Response = fileUploadModel;
                } 
                else
                {
                    answers.Add(new Answers { QuestionId = file.Key, Response = JsonConvert.SerializeObject(fileUploadModel) });
                }
            });

            return answers;
        }
    }
}
