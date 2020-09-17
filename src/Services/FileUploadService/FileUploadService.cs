using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly ISessionHelper _sessionHelper;
        private readonly IPageFactory _pageFactory;
        private readonly IPageHelper _pageHelper;

        public FileUploadService(IDistributedCacheWrapper distributedCache,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ISessionHelper sessionHelper,
            IPageFactory pageFactory, 
            IPageHelper pageHelper)
        {
            _distributedCache = distributedCache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _sessionHelper = sessionHelper;
            _pageFactory = pageFactory;
            _pageHelper = pageHelper;
        }

        public Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload)
        {
            fileUpload.Where(_ => _ != null)
                .ToList()
                .GroupBy(_ => _.QuestionId)
                .ToList()
                .ForEach((group) =>
                {
                    viewModel.Add(group.Key, group.Select(_ => new DocumentModel
                    {
                        Content =_.Base64EncodedContent, 
                        FileSize = _.Length
                    }).ToList());
                });

            return viewModel;
        }

        public ProcessRequestEntity RemoveFile(Dictionary<string, dynamic> viewModel,
            FormSchema baseForm,
            string path, 
            string sessionGuid)
        {
            var cachedAnswers = _distributedCache.GetString(sessionGuid);

            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var formDataKeyToRemove = convertedAnswers.FormData.FirstOrDefault(_ => _.Key.Contains(viewModel[FileUploadConstants.FileToDelete]));

            convertedAnswers.FormData.Remove(formDataKeyToRemove.Key);

            var pageAnswersString = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault();
            List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(pageAnswersString.Response.ToString());
            response.Remove(response.FirstOrDefault(_ => _.TrustedOriginalFileName.Equals(viewModel[FileUploadConstants.FileToDelete])));
            convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path)).Answers.FirstOrDefault().Response = response;

            _distributedCache.SetStringAsync(sessionGuid, JsonConvert.SerializeObject(convertedAnswers), CancellationToken.None);

            return new ProcessRequestEntity
            {
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path,
                    subPath = FileUploadConstants.SelectedFiles
                }
            };
        }

        public async Task<ProcessRequestEntity> ProcessFile(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            IEnumerable<CustomFormFile> files)
        {
            viewModel.TryGetValue(FileUploadConstants.SubPathViewModelKey, out var subPath);

            return viewModel.ContainsKey(FileUploadConstants.FileToDelete) ? RemoveFile(viewModel, baseForm, path, guid) : await ProcessSelectedFiles(viewModel, currentPage, baseForm, guid, path, files);
        }

        private async Task<ProcessRequestEntity> ProcessSelectedFiles(
         Dictionary<string, dynamic> viewModel,
         Page currentPage,
         FormSchema baseForm,
         string guid,
         string path,
         IEnumerable<CustomFormFile> files)
        {
            var element = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.MultipleFileUpload));
            
            if (!currentPage.IsValid)
            {
                var formModel = await _pageFactory.Build(currentPage, viewModel, baseForm, guid, null);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            var model = (viewModel[$"{element.Properties.QuestionId}{FileUploadConstants.SUFFIX}"] as IEnumerable<object>).ToList();
            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, files, currentPage.IsValid);

            if (model.Any())
            {
                return new ProcessRequestEntity
                {
                    RedirectToAction = true,
                    RedirectAction = "Index",
                    RouteValues = new
                    {
                        form = baseForm.BaseURL,
                        path,
                        subPath = FileUploadConstants.SelectedFiles
                    }
                };
            }
            return new ProcessRequestEntity
            {
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path,
                    subPath = ""
                }
            };
        }

     
    }
}
