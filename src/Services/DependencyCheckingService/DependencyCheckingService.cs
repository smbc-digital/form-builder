using form_builder.DependencyChecks;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Providers.EnabledFor;

namespace form_builder.Services.DependencyCheckingService
{
    public interface IDependencyCheckingService
    {
        Task<bool> IsAvailable(List<DependencyCheck> checks);
    }

    public class DependencyCheckingService : IDependencyCheckingService
    {
        private readonly IEnumerable<IDependencyCheck> _dependecyChecks;        

        public DependencyCheckingService(IEnumerable<IDependencyCheck> dependencyChecks)
        {
            _dependecyChecks = dependencyChecks;
        }

        public async Task<bool> IsAvailable(List<DependencyCheck> checks)
        {
            if(checks is null 
                || !checks.Any()
                || _dependecyChecks is null
                || !_dependecyChecks.Any())
                return true;
            
            foreach(DependencyCheck check in checks)
            {
                var relevantCheck = _dependecyChecks.Single(dc => dc.Name == check.Name);
                if (!await relevantCheck.IsAvailable())
                    return false;
            }

            return true;
        }
    }
}
