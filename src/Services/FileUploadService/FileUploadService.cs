using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.FileUploadService
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        //private readonly IPageHelper _pageHelper;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ISessionHelper _sessionHelper;
        //private readonly IPageFactory _pageFactory;

        public FileUploadService(IDistributedCacheWrapper distributedCache,
            //IPageHelper pageHelper,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ISessionHelper sessionHelper)
        {
            _distributedCache = distributedCache;
            //_pageHelper = pageHelper;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _sessionHelper = sessionHelper;
            //_pageFactory = pageFactory;
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

        //public async Task<ProcessRequestEntity> ProcessFile(
        //    Dictionary<string, dynamic> viewModel,
        //    Page currentPage,
        //    FormSchema baseForm,
        //    string guid,
        //    string path)
        //{
        //    viewModel.TryGetValue(FileUploadConstants.SubPathViewModelKey, out var subPath);
        //    return await ProcessSelectedFiles(viewModel, currentPage, baseForm, guid, path);
        //}

        //private async Task<ProcessRequestEntity> ProcessSelectedFiles(
        // Dictionary<string, dynamic> viewModel,
        // Page currentPage,
        // FormSchema baseForm,
        // string guid,
        // string path)
        //{
        //    var cachedAnswers = _distributedCache.GetString(guid);

        //    var convertedAnswers = cachedAnswers == null
        //        ? new FormAnswers { Pages = new List<PageAnswers>() }
        //        : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        //    var multipleFileElement = (convertedAnswers.FormData[$"{path}{FileUploadConstants.SelectedFiles}"] as IEnumerable<object>).ToList();

        //    if (!currentPage.IsValid)
        //    {
        //        var formModel = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null);

        //        return new ProcessRequestEntity
        //        {
        //            Page = currentPage,
        //            ViewModel = formModel
        //        };
        //    }

        //    if (multipleFileElement.Any())
        //    {
        //        return new ProcessRequestEntity
        //        {
        //            RedirectToAction = true,
        //            RedirectAction = "Index",
        //            RouteValues = new
        //            {
        //                form = baseForm.BaseURL,
        //                path,
        //                subPath = FileUploadConstants.SelectedFiles
        //            }
        //        };
        //    }
        //    return new ProcessRequestEntity
        //    {
        //        RedirectToAction = true,
        //        RedirectAction = "Index",
        //        RouteValues = new
        //        {
        //            form = baseForm.BaseURL,
        //            path,
        //            subPath = FileUploadConstants.Upload
        //        }
        //    };
        //}       

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
