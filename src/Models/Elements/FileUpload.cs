using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using StockportGovUK.NetStandard.Models.Models.Verint.Lookup;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace form_builder.Models.Elements
{
    public class FileUpload : Element
    {
        public FileUpload()
        {
            Type = EElementType.FileUpload;
        }

        public override Dictionary<string, dynamic> GenerateElementProperties()
        {
            var properties = new Dictionary<string, dynamic>()
            {
                { "name", Properties.QuestionId },
                { "id", Properties.QuestionId },
                {"type", "file" }
            };

            if (DisplayAriaDescribedby)
            {
                properties.Add("aria-describedby", DescribedByValue());
            }

            return properties;
        }


    }
}
