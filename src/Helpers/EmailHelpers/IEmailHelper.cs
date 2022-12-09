﻿using form_builder.Configuration;

namespace form_builder.Helpers.EmailHelpers
{
    public interface IEmailHelper
    {
        Task<EmailConfig> GetEmailInformation(string form);
    }
}
