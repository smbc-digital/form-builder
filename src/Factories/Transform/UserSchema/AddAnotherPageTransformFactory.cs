using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Builders;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Models.Elements;
using Newtonsoft.Json;

namespace form_builder.Factories.Transform.UserSchema
{
    public class AddAnotherPageTransformFactory : IUserPageTransformFactory
    {
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;

        public AddAnotherPageTransformFactory(IPageHelper pageHelper, ISessionHelper sessionHelper)
        {
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
        }

        public Page Transform(Page page, string sessionGuid)
        {
            var newListOfElements = new List<IElement>();
            foreach (var element in page.Elements)
            {
                if (element.Type.Equals(EElementType.AddAnother))
                {
                    newListOfElements.AddRange(GenerateListOfIncrementedElements(page.Elements, sessionGuid));
                }

                newListOfElements.Add(element);
            }

            page.Elements = newListOfElements;

            return page;
        }

        private IEnumerable<IElement> GenerateListOfIncrementedElements(IReadOnlyCollection<IElement> currentPageElements, string sessionGuid)
        {
            var addAnotherElement = currentPageElements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var addAnotherReplacementElements = new List<IElement>();
            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = _sessionHelper.GetSessionGuid();
                if (string.IsNullOrEmpty(sessionGuid))
                {
                    sessionGuid = Guid.NewGuid().ToString();
                    _sessionHelper.SetSessionGuid(sessionGuid);
                }
            }

            var convertedAnswers = _pageHelper.GetSavedAnswers(sessionGuid);
            var formDataIncrementKey = $"addAnotherFieldset-{addAnotherElement.Properties.QuestionId}";
            var fieldsetIncrements = convertedAnswers.FormData.ContainsKey(formDataIncrementKey) ? int.Parse(convertedAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString()) : 1;

            foreach (var pageElement in currentPageElements)
            {
                if (pageElement.Type.Equals(EElementType.AddAnother))
                {
                    for (var i = 1; i <= fieldsetIncrements; i++)
                    {
                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Fieldset)
                            .WithOpeningTagValue(true)
                            .Build());

                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Legend)
                            .WithLabel(addAnotherElement.Properties.Label)
                            .Build());

                        foreach (var element in addAnotherElement.Properties.Elements)
                        {
                            var incrementedElement = JsonConvert.DeserializeObject<IElement>(JsonConvert.SerializeObject(element));
                            incrementedElement.Properties.QuestionId = $"{element.Properties.QuestionId}:{i}:";
                            incrementedElement.Properties.IsDynamicallyGeneratedElement = true;
                            addAnotherReplacementElements.Add(incrementedElement);
                        }

                        if (fieldsetIncrements > 1)
                        {
                            addAnotherReplacementElements.Add(new ElementBuilder()
                                .WithType(EElementType.Button)
                                .WithButtonId($"remove-{i}")
                                .WithButtonName($"remove-{i}")
                                .WithPropertyText("Remove")
                                .WithClassName("smbc-button--link")
                                .Build());
                        }

                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Fieldset)
                            .WithOpeningTagValue(false)
                            .Build());
                    }

                    if (fieldsetIncrements < addAnotherElement.Properties.MaximumFieldsets)
                    {
                        addAnotherReplacementElements.Add(new ElementBuilder()
                            .WithType(EElementType.Button)
                            .WithButtonId("addAnotherFieldset")
                            .WithButtonName("addAnotherFieldset")
                            .WithPropertyText("Add another")
                            .WithClassName("govuk-button--secondary")
                            .Build());
                    }
                }
            }

            return addAnotherReplacementElements;
        }
    }
}
