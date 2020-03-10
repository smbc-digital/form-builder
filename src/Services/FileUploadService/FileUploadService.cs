using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace form_builder.Services.FileUploadService
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileHelper _fileHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ICache _cache;

        public FileUploadService(IFileHelper fileHelper, IDistributedCacheWrapper distributedCache,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration, ICache cache)
        {
            _fileHelper = fileHelper;
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _cache = cache;
        }

        public Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IFormFileCollection fileUpload, string form, string path)
        {
            var baseForm = _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(form,
                _distributedCacheExpirationConfiguration.FormJson, ESchemaType.FormJson).Result;

            var page = baseForm.GetPage(path);
            var elements = page.Elements.Where((element, i) => element.Type == EElementType.FileUpload).ToList();

            for (int fileCount = 0; fileCount < fileUpload.Count; fileCount++)
            {
                if (elements!= null && elements[fileCount] != null)
                    viewModel.Add(fileUpload[fileCount].Name + $"_{elements[fileCount].Properties.QuestionId}", _fileHelper.ConvertFileToBase64StringWithFileName(fileUpload[fileCount]));
            }
            return viewModel;
        }

        public FormAnswers CollectAnswers(FormAnswers currentPageAnswers, IFormFileCollection files, Dictionary<string,dynamic> viewModel)
        {
            if (files.Any())
            {
                for (int fileCount = 0; fileCount < files.Count(); fileCount++)
                {
                    var finalAnswers = currentPageAnswers.Pages
                        .FirstOrDefault(_ => _.PageSlug == viewModel["Path"].ToLower());

                    var file = files[fileCount];
                    if (finalAnswers != null)
                    {
                        var fileName = $"{finalAnswers.Answers[fileCount].QuestionId}"; // this is needed to get the filename
                        var fileKey = $"file-{fileName}"; // this is the key for storing in the cache

                        _distributedCache.SetStringAsync(fileKey, viewModel[fileName].Content, _distributedCacheExpirationConfiguration.FileUpload);
                        FileUploadModel model = new FileUploadModel
                        {
                            FileName = viewModel[fileName].FileName,
                            Key = fileKey
                        };

                        if (finalAnswers.Answers.Exists(_ => _.QuestionId == fileName))
                        {
                            var fileUploadQuestion = finalAnswers.Answers.First(_ => _.QuestionId == fileName);
                            finalAnswers.Answers.Remove(fileUploadQuestion);
                        }
                        finalAnswers.Answers.Add(new Answers { QuestionId = fileName, Response = model });
                    }
                }
            }

            return currentPageAnswers;
        }
    }
}
