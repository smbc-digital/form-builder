using System.Threading.Tasks;
using form_builder.Enum;
using form_builder.Services.PageService.Entities;

namespace form_builder.Workflows.SuccessWorkflow
{
    public interface ISuccessWorkflow
    {
        Task<SuccessPageEntity> Process(EBehaviourType behaviourType, string form);
    }
}
