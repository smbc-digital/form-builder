namespace form_builder_tests.Builders
{
    public class ViewModelBuilder
    {
        private Dictionary<string, string[]> _viewModel = new Dictionary<string, string[]>();

        public Dictionary<string, string[]> Build()
        {
            return _viewModel;
        }

        public ViewModelBuilder WithEntry(string key, string value)
        {
            _viewModel.Add(key, new[] { value });

            return this;
        }
    }
}