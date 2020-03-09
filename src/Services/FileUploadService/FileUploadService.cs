using System.Collections.Generic;
using System.Linq;
using form_builder.Configuration;
using form_builder.Helpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Http;

namespace form_builder.Services.FileUploadService
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileHelper _fileHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public FileUploadService(IFileHelper fileHelper, IDistributedCacheWrapper distributedCache, 
            DistributedCacheExpirationConfiguration distributedCacheExpirationConfiguration)
        {
            _fileHelper = fileHelper;
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration;
        }

        public Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IFormFileCollection fileUpload)
        {
            for (int i = 0; i < fileUpload.Count; i++)
            {
                viewModel.Add(fileUpload[i].Name + $"_{i}", _fileHelper.ConvertFileToBase64StringWithFileName(fileUpload[i]));
            }
            return viewModel;
        }

        public FormAnswers CollectAnswers(FormAnswers currentPageAnswers, IFormFileCollection files, Dictionary<string,dynamic> viewModel)
        {
            if (files.Any())
            {
                for (int fileCount = 0; fileCount < files.Count(); fileCount++)
                {
                    var file = files[fileCount];
                    var fileName = file.Name + $"_{fileCount}";
                    var fileKey = $"file-{fileName}";

                    _distributedCache.SetStringAsync(fileKey, viewModel[fileName].Content, _distributedCacheExpirationConfiguration.FileUpload);
                    FileUploadModel model = new FileUploadModel
                    {
                        FileName = viewModel[fileName].FileName,
                        Key = fileKey
                    };
                    var finalAnswers = currentPageAnswers.Pages.Where(_ => _.PageSlug == viewModel["Path"].ToLower())
                        .FirstOrDefault();
                    if (finalAnswers.Answers.Exists(_ => _.QuestionId == fileName))
                    {
                        var fileUploadQuestion = finalAnswers.Answers.First(_ => _.QuestionId == fileName);
                        finalAnswers.Answers.Remove(fileUploadQuestion);
                    }
                    finalAnswers.Answers.Add(new Answers { QuestionId = fileName, Response = model });
                }
            }

            return currentPageAnswers;
        }
    }
}
