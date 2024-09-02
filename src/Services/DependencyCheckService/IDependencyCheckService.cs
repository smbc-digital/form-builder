namespace form_builder.Services.DependencyCheckService
{
    public interface IDependencyCheckService
    {
        Task<bool> IsAvailable(List<string> CheckList);
    }
}
