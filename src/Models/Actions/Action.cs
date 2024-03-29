﻿using form_builder.Enum;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models.Properties.ActionProperties;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.TemplatedEmailProvider;

namespace form_builder.Models.Actions
{
#pragma warning disable CS1998
    public class Action : IAction
    {
        public EActionType Type { get; set; }

        public BaseActionProperty Properties { get; set; }

        public virtual async Task Process(IActionHelper actionHelper, IEmailProvider emailProvider, FormAnswers formAnswers)
        {
            throw new System.NotImplementedException();
        }

        public virtual Task ProcessTemplatedEmail(IActionHelper actionHelper, ITemplatedEmailProvider templatedEmailProvider, Dictionary<string, dynamic> personalisation, FormAnswers formAnswers)
        {
            throw new System.NotImplementedException();
        }
    }
}