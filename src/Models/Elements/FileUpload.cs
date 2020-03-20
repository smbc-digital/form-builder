using System;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using form_builder.Constants;

namespace form_builder.Models.Elements
{
    public class FileUpload : Element
    {
        public FileUpload()
        {
            Type = EElementType.FileUpload;
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type)
        {
            var allowedFileType = Properties.AllowedFileTypes ?? SystemConstants.AcceptedMimeTypes;
            var maxFileSize = Properties.MaxFileSize > 0 ? Properties.MaxFileSize*1024000 : SystemConstants.DefaultMaxFileSize;
            maxFileSize = maxFileSize > SystemConstants.DefaultMaxFileSize
                ? SystemConstants.DefaultMaxFileSize
                : Properties.MaxFileSize * 1024000;

            var properties = new Dictionary<string, dynamic>()
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                { "type", "file" },
                { "accept", allowedFileType.Join(",") },
                { "max-file-size", maxFileSize },
                { "onchange", "ValidateSize(this)" }
            };

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", DescribedByValue());
            }

            return properties;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid, string.Empty);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }
    }
}
