using System;
using System.Collections.Generic;
using System.Linq;
using form_builder.Conditions;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Actions;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Validators;
using Newtonsoft.Json;

namespace form_builder.Models
{
	public class Page
	{
		public Page()
		{
			IsValidated = false;
		}

		public string Title { get; set; }

		public string PageSlug { get; set; }

		public bool DisplayBreadCrumbs { get; set; }

		public List<IElement> Elements { get; set; }

		public List<Behaviour> Behaviours { get; set; }

		public List<IncomingValue> IncomingValues { get; set; } = new List<IncomingValue>();

		public bool IsValidated { get; set; }

		public bool HideTitle { get; set; }

		public bool HideBackButton { get; set; }

		public string BannerTitle { get; set; }

		public string LeadingParagraph { get; set; }

		[JsonIgnore] public bool IsValid => !InvalidElements.Any();

		public bool HasIncomingValues => IncomingValues.Any();
		public bool HasIncomingGetValues => IncomingValues.Any(_ => _.HttpActionType == EHttpActionType.Get);
		public bool HasIncomingPostValues => IncomingValues.Any(_ => _.HttpActionType == EHttpActionType.Post);

		public List<IAction> PageActions { get; set; } = new List<IAction>();

		public bool HasPageActions => PageActions.Any();

		public bool HasIncomingAction => PageActions.Any();
		public bool HasPageActionsGetValues => PageActions.Any(_ => _.Properties.HttpActionType == EHttpActionType.Get);
		public bool HasPageActionsPostValues => PageActions.Any(_ => _.Properties.HttpActionType == EHttpActionType.Post);


		public List<Condition> RenderConditions { get; set; } = new List<Condition>();

		public bool HasRenderConditions => RenderConditions.Any();

		[JsonIgnore]
		public IEnumerable<BaseProperty> InvalidElements
		{
			get
			{
				if (IsValidated)
				{
					IEnumerable<BaseProperty> invalidElements = ValidatableElements.Where(element => !element.IsValid).Select(element => element.Properties);

					// Check if Radio element or its relative conditional element is valid 
					//IEnumerable<BaseProperty> invalidRadioConditional = ValidatableElements.Where(element => element.Type == EElementType.Radio && (!element.IsValid || element.Properties.Options.Where(option => option.HasConditionalElement && !option.ConditionalElement.IsValid).Any())).Select(element => element.Properties);
					return invalidElements;
				}

				throw new Exception("Model is not validated, please call Validate()");
			}
		}

		public IEnumerable<IElement> ValidatableElements => Elements.Where(element =>
			element.Type == EElementType.Radio ||
			element.Type == EElementType.Textarea ||
			element.Type == EElementType.Select ||
			element.Type == EElementType.Textbox ||
			element.Type == EElementType.Checkbox ||
			element.Type == EElementType.Declaration ||
			element.Type == EElementType.Address ||
			element.Type == EElementType.AddressManual ||
			element.Type == EElementType.DateInput ||
			element.Type == EElementType.TimeInput ||
			element.Type == EElementType.DatePicker ||
			element.Type == EElementType.Street ||
			element.Type == EElementType.Organisation ||
			element.Type == EElementType.FileUpload ||
			element.Type == EElementType.MultipleFileUpload ||
			element.Type == EElementType.Map
		);

		public IEnumerable<IElement> ValidatableElementsConditional => Elements.Where(element =>
			element.Type == EElementType.Textarea ||
			element.Type == EElementType.Select ||
			element.Type == EElementType.Textbox ||
			element.Type == EElementType.DateInput ||
			element.Type == EElementType.TimeInput
		);

		public void Validate(Dictionary<string, dynamic> viewModel, IEnumerable<IElementValidator> form_builder)
		{
			//ValidatableElements.IncludedRequiredConditionalElements(viewModel).ForEach(element => {
			//	element.Validate(viewModel, form_builder);
			//});

			ValidatableElements.RemoveUnusedConditionalElements(viewModel).ForEach(element => {
				element.Validate(viewModel, form_builder);
			});
			IsValidated = true;
		}

		public Behaviour GetNextPage(Dictionary<string, dynamic> viewModel)
		{
			var conditionValidator = new ConditionValidator();

			if (Behaviours.Count == 1)
				return Behaviours.FirstOrDefault();

			foreach (var behaviour in Behaviours.OrderByDescending(_ => _.Conditions.Count))
			{
				var isConditionTrue = false;

				foreach (var condition in behaviour.Conditions)
				{
					isConditionTrue = conditionValidator.IsValid(condition, viewModel);

					if (!isConditionTrue)
						break;
				}

				if (isConditionTrue || !behaviour.Conditions.Any())
					return behaviour;
			}

			throw new Exception("Page::GetNextPage, There was a problem whilst processing behaviors");
		}

		public bool CheckPageMeetsConditions(Dictionary<string, dynamic> answers)
		{
			var conditionValidator = new ConditionValidator();

			return RenderConditions.Count == 0 ||
				   RenderConditions.All(condition => conditionValidator.IsValid(condition, answers));
		}

		public SubmitSlug GetSubmitFormEndpoint(FormAnswers formAnswers, string environment)
		{
			var submitBehaviour = new SubmitSlug();

			var pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitForm);
			if (pageSubmitBehaviours.Count == 0)
				pageSubmitBehaviours = GetBehavioursByType(EBehaviourType.SubmitAndPay);

			if (Behaviours.Count > 1)
			{
				var previousPage = formAnswers.Pages
					.SelectMany(_ => _.Answers)
					.ToList();

				var viewModel = new Dictionary<string, dynamic>();
				previousPage.ForEach(_ => viewModel.Add(_.QuestionId, _.Response));
				var foundSubmitBehaviour = GetNextPage(viewModel);

				submitBehaviour = foundSubmitBehaviour.SubmitSlugs
					.ToList()
					.FirstOrDefault(x => x.Environment.ToLower().Equals(environment.ToLower()));
			}
			else
			{
				if (pageSubmitBehaviours.FirstOrDefault()?.SubmitSlugs.Count == 0)
				{
					submitBehaviour.URL = pageSubmitBehaviours.FirstOrDefault()?.PageSlug;
				}
				else
				{
					var behaviour = pageSubmitBehaviours
						.SelectMany(x => x.SubmitSlugs)
						.FirstOrDefault(x => x.Environment.ToLower().Equals(environment.ToLower()));

					submitBehaviour = behaviour ?? throw new NullReferenceException("Page model::GetSubmitFormEndpoint, No Url supplied for submit form");
				}
			}

			return string.IsNullOrEmpty(submitBehaviour.URL)
				? throw new NullReferenceException(
					"Page model::GetSubmitFormEndpoint, No postUrl supplied for submit form")
				: submitBehaviour;
		}

		private List<Behaviour> GetBehavioursByType(EBehaviourType type) => Behaviours.Where(_ => _.BehaviourType == type).ToList();

		public string GetPageTitle() => Elements.Any() && HideTitle ? Elements.First().Properties.Label : Title;
	}
}