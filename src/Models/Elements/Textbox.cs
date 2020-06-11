﻿using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class Textbox : Element
    {
        public Textbox()
        {
            Type = EElementType.Textbox;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var properties = new Dictionary<string, dynamic>()
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                { "maxlength", Properties.MaxLength },
                { "value", Properties.Value},
                { "spellcheck", Properties.Spellcheck.ToString() }
            };
            
            if (Properties.Numeric)
            {
                properties.Add("type", "number");
                properties.Add("max", Properties.Max);
                properties.Add("min", Properties.Min);
            }

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", GetDescribedByAttributeValue());
            }

            if (!string.IsNullOrEmpty(Properties.Purpose))
            {
                properties.Add("autocomplete", Properties.Purpose);
            }

            return properties;
        }
    }
}
