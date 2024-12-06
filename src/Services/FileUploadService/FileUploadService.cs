using System.Net;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.FileStorage;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.FileUploadService;

public class FileUploadService : IFileUploadService
{
    private readonly IDistributedCacheWrapper _distributedCache;
    private readonly IFileStorageProvider _fileStorageProvider;
    private readonly IPageFactory _pageFactory;
    private readonly IPageHelper _pageHelper;

    public FileUploadService(IDistributedCacheWrapper distributedCache,
        IEnumerable<IFileStorageProvider> fileStorageProviders,
        IPageFactory pageFactory,
        IPageHelper pageHelper,
        IOptions<FileStorageProviderConfiguration> fileStorageConfiguration)
    {
        _distributedCache = distributedCache;
        _pageFactory = pageFactory;
        _pageHelper = pageHelper;
        _fileStorageProvider = fileStorageProviders.Get(fileStorageConfiguration.Value.Type);
    }

    public Dictionary<string, dynamic> AddFiles(Dictionary<string, dynamic> viewModel, IEnumerable<CustomFormFile> fileUpload)
    {
        fileUpload
            .Where(_ => _ is not null)
            .GroupBy(_ => _.QuestionId)
            .ToList()
            .ForEach(group =>
            {
                viewModel.Add(group.Key, group.Select(_ => new DocumentModel
                {
                    Content = _.Base64EncodedContent,
                    FileSize = _.Length,
                    FileName = WebUtility.HtmlEncode(_.UntrustedOriginalFileName)
                }).ToList());
            });

        return viewModel;
    }

    public async Task<ProcessRequestEntity> ProcessFile(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path,
        IEnumerable<CustomFormFile> files,
        bool modelStateIsValid)
    {
        return viewModel.ContainsKey(FileUploadConstants.FILE_TO_DELETE)
            ? RemoveFile(viewModel, baseForm, path, cacheKey)
            : await ProcessSelectedFiles(viewModel, currentPage, baseForm, cacheKey, path, files, modelStateIsValid);
    }

    private ProcessRequestEntity RemoveFile(
        Dictionary<string, dynamic> viewModel,
        FormSchema baseForm,
        string path,
        string cacheKey)
    {
        var cachedAnswers = _distributedCache.GetString(cacheKey);

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var currentPageAnswers = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault();
        List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(currentPageAnswers.Response.ToString());

        var fileToRemove = response.FirstOrDefault(_ => _.TrustedOriginalFileName.Equals(viewModel[FileUploadConstants.FILE_TO_DELETE]));
        response.Remove(fileToRemove);
        convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path)).Answers.FirstOrDefault().Response = response;

        _distributedCache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(convertedAnswers), CancellationToken.None);
        _fileStorageProvider.Remove(fileToRemove.Key);

        return new ProcessRequestEntity
        {
            RedirectToAction = true,
            RedirectAction = "Index",
            RouteValues = new
            {
                form = baseForm.BaseURL,
                path
            }
        };
    }

    private async Task<ProcessRequestEntity> ProcessSelectedFiles(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path,
        IEnumerable<CustomFormFile> files,
        bool modelStateIsValid)
    {
        if (!currentPage.IsValid)
        {
            var formModel = await _pageFactory.Build(currentPage, new Dictionary<string, dynamic>(), baseForm, cacheKey);

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = formModel
            };
        }

        if (currentPage.IsValid && viewModel.ContainsKey(ButtonConstants.SUBMIT) && (files is null || !files.Any()) && modelStateIsValid)
        {
            if (currentPage.Elements.Where(_ => _.Type.Equals(EElementType.MultipleFileUpload)).Any(_ => _.Properties.Optional))
                _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, files, currentPage.IsValid, true);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!viewModel.ContainsKey(ButtonConstants.SUBMIT) && (files is null || !files.Any()))
        {
            return new ProcessRequestEntity
            {
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path
                }
            };
        }

        if (files is not null && files.Any())
            _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, files, currentPage.IsValid, true);

        if (viewModel.ContainsKey(ButtonConstants.SUBMIT) && modelStateIsValid)
        {
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!modelStateIsValid)
        {
            var newViewModel = new Dictionary<string, dynamic>
            {
                { "modelStateInvalid", null }
            };

            var formModel = await _pageFactory.Build(currentPage, newViewModel, baseForm, cacheKey);

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = formModel,
                UseGeneratedViewModel = true
            };
        }

        return new ProcessRequestEntity
        {
            RedirectToAction = true,
            RedirectAction = "Index",
            RouteValues = new
            {
                form = baseForm.BaseURL,
                path
            }
        };
    }
}