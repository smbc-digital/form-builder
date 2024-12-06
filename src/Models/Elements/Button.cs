using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.ViewRender;

namespace form_builder.Models.Elements;

public class Button : Element
{
    public Button() => Type = EElementType.Button;

    public override Task<string> RenderAsync(IViewRender viewRender,
        IElementHelper elementHelper,
        string cacheKey,
        Dictionary<string, dynamic> viewModel,
        Page page,
        FormSchema formSchema,
        IWebHostEnvironment environment,
        FormAnswers formAnswers,
        List<object> results = null)
    {
        Properties.Text = GetButtonText(page.Elements, viewModel, page);

        if (!Properties.DisableOnClick)
            Properties.DisableOnClick = DisableIfSubmitOrLookup(page.Behaviours, page.Elements, viewModel);

        return viewRender.RenderAsync("Button", this);
    }

    private bool CheckForBehaviour(List<Behaviour> behaviour)
    {
        return behaviour.Any(_ => _.BehaviourType.Equals(EBehaviourType.SubmitForm) || _.BehaviourType.Equals(EBehaviourType.SubmitAndPay));
    }

    private bool CheckForLookups(List<IElement> element, Dictionary<string, dynamic> viewModel)
    {
        var containsLookupElement = element.Any(_ => _.Type.Equals(EElementType.Address) || _.Type.Equals(EElementType.Street) || _.Type.Equals(EElementType.Organisation));

        if (containsLookupElement && !viewModel.IsInitial())
            return false;

        return containsLookupElement;
    }

    private bool DisableIfSubmitOrLookup(List<Behaviour> behaviour, List<IElement> element, Dictionary<string, dynamic> viewModel)
    {
        return CheckForBehaviour(behaviour) || CheckForLookups(element, viewModel);
    }

    private string GetButtonText(List<IElement> element, Dictionary<string, dynamic> viewModel, Page page)
    {
        if (element.Any(_ => _.Type.Equals(EElementType.Address)) && viewModel.IsInitial())
            return ButtonConstants.ADDRESS_SEARCH_TEXT;

        if (element.Any(_ => _.Type.Equals(EElementType.Street)) && viewModel.IsInitial())
            return ButtonConstants.STREET_SEARCH_TEXT;

        if (element.Any(_ => _.Type.Equals(EElementType.Organisation)) && viewModel.IsInitial())
            return ButtonConstants.ORG_SEARCH_TEXT;

        if (element.Any(_ => _.Type.Equals(EElementType.Booking)))
        {
            var bookingElement = element.FirstOrDefault(_ => _.Type.Equals(EElementType.Booking));

            if (bookingElement.Properties.CheckYourBooking && !viewModel.IsCheckYourBooking())
                return string.IsNullOrEmpty(Properties.Text) ? ButtonConstants.NEXTSTEP_TEXT : Properties.Text;
        }

        if (page.Behaviours.Any(_ => _.BehaviourType.Equals(EBehaviourType.SubmitForm) || _.BehaviourType.Equals(EBehaviourType.SubmitAndPay)))
            return string.IsNullOrEmpty(Properties.Text) ? ButtonConstants.SUBMIT_TEXT : Properties.Text;

        return string.IsNullOrEmpty(Properties.Text) ? ButtonConstants.NEXTSTEP_TEXT : Properties.Text;
    }
}