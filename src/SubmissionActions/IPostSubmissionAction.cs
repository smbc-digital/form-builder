using form_builder.Models;
using form_builder.Services.MappingService.Entities;
using System.Threading.Tasks;

namespace form_builder.SubmissionActions
{
    public interface IPostSubmissionAction
    {
        Task ConfirmBooking(MappingEntity mappingEntity, FormSchema baseForm, string environmentName);
    }
}
