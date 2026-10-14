namespace form_builder_tests.Builders;

public class ViewModelBuilder
{
    private readonly Dictionary<string, string[]> _viewModel = new();

    public Dictionary<string, string[]> Build()
    {
        return _viewModel;
    }

    public ViewModelBuilder WithEntry(string key, string value)
    {
        _viewModel.Add(key, [value]);

        return this;
    }
}