﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.FileStorage;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;

namespace form_builder.Services.FileUploadService
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IFileStorageProvider _fileStorage;
        private readonly IPageFactory _pageFactory;
        private readonly IPageHelper _pageHelper;

        public FileUploadService(IFileStorageProvider fileStorage,
            IPageFactory pageFactory,
            IPageHelper pageHelper)
        {
            _fileStorage = fileStorage;
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
            string guid,
            string path,
            IEnumerable<CustomFormFile> files,
            bool modelStateIsValid)
        {
            return viewModel.ContainsKey(FileUploadConstants.FILE_TO_DELETE)
                ? RemoveFile(viewModel, baseForm, path, guid)
                : await ProcessSelectedFiles(viewModel, currentPage, baseForm, guid, path, files, modelStateIsValid);
        }

        private ProcessRequestEntity RemoveFile(
            Dictionary<string, dynamic> viewModel,
            FormSchema baseForm,
            string path,
            string sessionGuid)
        {
            var cachedAnswers = _fileStorage.GetString(sessionGuid);

            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var currentPageAnswers = convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path))?.Answers.FirstOrDefault();
            List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(currentPageAnswers.Response.ToString());

            var fileToRemove = response.FirstOrDefault(_ => _.TrustedOriginalFileName.Equals(viewModel[FileUploadConstants.FILE_TO_DELETE]));
            response.Remove(fileToRemove);
            convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(path)).Answers.FirstOrDefault().Response = response;

            _fileStorage.SetStringAsync(sessionGuid, JsonConvert.SerializeObject(convertedAnswers), CancellationToken.None);
            _fileStorage.Remove(fileToRemove.Key);

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
         string guid,
         string path,
         IEnumerable<CustomFormFile> files,
         bool modelStateIsValid)
        {
            if (!currentPage.IsValid)
            {
                var formModel = await _pageFactory.Build(currentPage, new Dictionary<string, dynamic>(), baseForm, guid);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            if (currentPage.IsValid && viewModel.ContainsKey(ButtonConstants.SUBMIT) && (files == null || !files.Any()) && modelStateIsValid)
            {
                if (currentPage.Elements.Where(_ => _.Type.Equals(EElementType.MultipleFileUpload)).Any(_ => _.Properties.Optional))
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, files, currentPage.IsValid, true);

                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            if (!viewModel.ContainsKey(ButtonConstants.SUBMIT) && (files == null || !files.Any()))
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

            if (files != null && files.Any())
                _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, files, currentPage.IsValid, true);

            if (viewModel.ContainsKey(ButtonConstants.SUBMIT) && modelStateIsValid)
            {
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            if (!modelStateIsValid)
            {
                var newViewModel = new Dictionary<string, dynamic>();
                newViewModel.Add("modelStateInvalid", null);

                var formModel = await _pageFactory.Build(currentPage, newViewModel, baseForm, guid);

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
}
