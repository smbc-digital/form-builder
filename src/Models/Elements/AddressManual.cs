using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Extensions;

namespace form_builder.Models.Elements
{
    public class AddressManual : Element, IElement
    {
        public AddressManual()
        {
            Type = EElementType.AddressManual;
        }

        
    }
}
