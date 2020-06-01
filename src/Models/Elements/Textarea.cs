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
    public class Textarea : Element
    {
        public bool DisplayCharacterCount => Properties.DisplayCharacterCount;
        public Textarea()
        {
            Type = EElementType.Textarea;
        }

        public override Task<string> RenderAsync(IViewRender viewRender, IElementHelper elementHelper, string guid, List<AddressSearchResult> addressSearchResults, List<OrganisationSearchResult> organisationResults, Dictionary<string, dynamic> viewModel, Page page, FormSchema formSchema, IHostingEnvironment environment)
        {
            Properties.Value = elementHelper.CurrentValue(this, viewModel, page.PageSlug, guid);
            elementHelper.CheckForQuestionId(this);
            elementHelper.CheckForLabel(this);
            elementHelper.CheckForMaxLength(this);
            return viewRender.RenderAsync(Type.ToString(), this);
        }

        public override Dictionary<string, dynamic> GenerateElementProperties(string type = "")
        {
            var properties = new Dictionary<string, dynamic>()
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                { "value", Properties.Value},
                { "spellcheck", Properties.Spellcheck.ToString().ToLower() }
            };
            
            properties.Add("maxlength", Properties.MaxLength);
            if (Properties.MaxLength >= 200 && Properties.MaxLength <= 500)
            {
                properties.Add("rows", "5");
            }

            if (Properties.MaxLength > 500)
            {
                properties.Add("rows", "15");
            }

            if (DisplayAriaDescribedby)
            {
                if(DisplayCharacterCount)
                {
                    properties.Add("aria-describedby", $"{GetCustomItemId("info")} {GetDescribedByAttributeValue()}");
                }
                else
                {
                    properties.Add("aria-describedby", GetDescribedByAttributeValue());
                }    
            }

            return properties;
        }
    }
}
