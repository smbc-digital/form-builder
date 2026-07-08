namespace form_builder.Builders;

public class PageBuilder
{
    private string _title = "TestTitle";
    private bool _hideTitle = false;
    private bool _hideBackButton = false;
    private string _pageSlug = "test-url";
    private string _leadingParagraph = "Leading paragraph";
    private bool _isValidated = false;
    private List<IElement> _elements = new();
    private List<Behaviour> _behaviours = new();
    private List<IncomingValue> _incomingValues = new();
    private List<IAction> _pageActions = new();
    private List<Condition> _renderConditions = new();

    public Page Build()
    {
        return new Page
        {
            Title = _title,
            HideTitle = _hideTitle,
            HideBackButton = _hideBackButton,
            LeadingParagraph = _leadingParagraph,
            PageSlug = _pageSlug,
            IsValidated = _isValidated,
            Behaviours = _behaviours,
            Elements = _elements,
            IncomingValues = _incomingValues,
            PageActions = _pageActions,
            RenderConditions = _renderConditions
        };
    }

    public PageBuilder WithPageTitle(string title)
    {
        _title = title;

        return this;
    }

    public PageBuilder WithValidatedModel(bool value)
    {
        _isValidated = value;

        return this;
    }

    public PageBuilder WithPageSlug(string url)
    {
        _pageSlug = url;

        return this;
    }

    public PageBuilder WithElement(Element element)
    {
        _elements.Add(element);

        return this;
    }

    public PageBuilder WithHideTitle(bool value)
    {
        _hideTitle = value;

        return this;
    }

    public PageBuilder WithHideBackButton(bool value)
    {
        _hideBackButton = value;

        return this;
    }

    public PageBuilder WithBehaviour(Behaviour behaviour)
    {
        _behaviours.Add(behaviour);

        return this;
    }

    public PageBuilder WithIncomingValue(IncomingValue value)
    {
        _incomingValues.Add(value);

        return this;
    }

    public PageBuilder WithPageActions(IAction pageAction)
    {
        _pageActions.Add(pageAction);

        return this;
    }

    public PageBuilder WithRenderConditions(Condition renderCondition)
    {
        _renderConditions.Add(renderCondition);

        return this;
    }

    public PageBuilder WithLeadingParagraph(string leadingParagraph)
    {
        _leadingParagraph = leadingParagraph;

        return this;
    }
}