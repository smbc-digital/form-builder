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
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Server.Kestrel.Core.Adapter.Internal;
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

            for (int fileCount = 0; fileCount < elements.Count; fileCount++)
            {
                if (elements != null && elements[fileCount] != null)
                {
                    if (fileCount < fileUpload.Count)
                    {
                        var fileContent = _fileHelper.ConvertFileToBase64StringWithFileName(fileUpload[fileCount]);
                        viewModel.Add(fileUpload[fileCount].Name + $"_{elements[fileCount].Properties.QuestionId}",
                            fileContent);
                    }
                    else
                    {
                        viewModel.Add($"_{elements[fileCount].Properties.QuestionId}", string.Empty);
                    }
                }
            }
            return viewModel;
        }

        public FormAnswers CollectAnswers(FormAnswers currentPageAnswers, IFormFileCollection files, Dictionary<string,dynamic> viewModel)
        {
            for (int fileCount = 0; fileCount < files.Count(); fileCount++)
            {
                var finalAnswers = currentPageAnswers.Pages
                    .FirstOrDefault(_ => _.PageSlug == viewModel["Path"].ToLower());
                if (finalAnswers != null)
                {
                    var fileName = $"{finalAnswers.Answers[fileCount].QuestionId}"; // this is needed to get the filename
                    var fileKey = $"file-{fileName}"; // this is the key for storing in the cache

                    if (viewModel[fileName] != null)
                    {
                        _distributedCache.SetStringAsync(fileKey, viewModel[fileName].Content,
                            _distributedCacheExpirationConfiguration.FileUpload);
                        FileUploadModel model = new FileUploadModel
                        {
                            FileName = viewModel[fileName].FileName,
                            Key = fileKey,
                            OriginalFileName = files[fileCount].FileName
                        };

                        if (finalAnswers.Answers.Exists(_ => _.QuestionId == fileName))
                        {
                            var fileUploadAnswer = finalAnswers.Answers.FirstOrDefault(_ => _.QuestionId == fileName);
                            if (fileUploadAnswer != null)
                                fileUploadAnswer.Response = model;
                        }
                    }
                }
            }

            return currentPageAnswers;
        }
    }
}
