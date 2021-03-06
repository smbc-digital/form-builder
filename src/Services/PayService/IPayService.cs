﻿using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Models;
using form_builder.Services.MappingService.Entities;

namespace form_builder.Services.PayService
{
    public interface IPayService
    {
        Task<string> ProcessPayment(MappingEntity mappingEntity, string form, string path, string reference, string sessionGuid);
        Task<string> ProcessPaymentResponse(string form, string responseCode, string reference);
        Task<PaymentInformation> GetFormPaymentInformation(MappingEntity mappingEntity, string form, Page page);
    }
}
